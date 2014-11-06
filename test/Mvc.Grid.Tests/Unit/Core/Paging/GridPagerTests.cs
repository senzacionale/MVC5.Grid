﻿using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace NonFactors.Mvc.Grid.Tests.Unit
{
    [TestFixture]
    public class GridPagerTests
    {
        private RequestContext requestContext;
        private GridPager<GridModel> pager;
        private IGrid<GridModel> grid;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            grid = Substitute.For<IGrid<GridModel>>();
            grid.Source.Returns(new GridModel[6].AsQueryable());
            requestContext = HttpContextFactory.CreateHttpContext().Request.RequestContext;

            pager = new GridPager<GridModel>(grid, requestContext);
        }

        #region Property: StartingPage

        [Test]
        [TestCase(1, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(1, 3, 3)]
        [TestCase(1, 4, 4)]
        [TestCase(1, 5, 5)]
        [TestCase(3, 0, 0)]
        [TestCase(3, 1, 0)]
        [TestCase(3, 2, 1)]
        [TestCase(3, 3, 2)]
        [TestCase(3, 4, 3)]
        [TestCase(3, 5, 3)]
        [TestCase(4, 0, 0)]
        [TestCase(4, 1, 0)]
        [TestCase(4, 2, 1)]
        [TestCase(4, 3, 2)]
        [TestCase(4, 4, 2)]
        [TestCase(4, 5, 2)]
        public void StartingPage_GetsStartingPage(Int32 pagesToDisplay, Int32 currentPage, Int32 startingPage)
        {
            pager.PagesToDisplay = pagesToDisplay;
            pager.CurrentPage = currentPage;
            pager.RowsPerPage = 1;

            Int32 actual = pager.StartingPage;
            Int32 expected = startingPage;

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Property: TotalPages

        [Test]
        [TestCase(0, 20, 0)]
        [TestCase(1, 20, 1)]
        [TestCase(19, 20, 1)]
        [TestCase(20, 20, 1)]
        [TestCase(21, 20, 2)]
        [TestCase(39, 20, 2)]
        [TestCase(40, 20, 2)]
        [TestCase(41, 20, 3)]
        public void TotalPages_GetsTotalPages(Int32 itemsCount, Int32 rowsPerPage, Int32 totalPages)
        {
            grid.Source.Returns(new GridModel[itemsCount].AsQueryable());

            GridPager<GridModel> pager = new GridPager<GridModel>(grid, null);
            pager.RowsPerPage = rowsPerPage;

            Int32 actual = pager.TotalPages;
            Int32 expected = totalPages;

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Constructor: GridPager(IGrid<TModel> grid, RequestContext requestContext)

        [Test]
        public void GridPager_SetsDefaultPartialViewName()
        {
            String actual = new GridPager<GridModel>(grid, null).PartialViewName;
            String expected = "MvcGrid/_Pager";

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsRequestContext()
        {
            RequestContext expected = HttpContextFactory.CreateHttpContext().Request.RequestContext;
            RequestContext actual = new GridPager<GridModel>(grid, expected).RequestContext;

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GridPager_SetsCurrentPageFromQuery()
        {
            grid.Query.GetPagingQuery().CurrentPage.Returns(15);

            Int32 actual = new GridPager<GridModel>(grid, null).CurrentPage;
            Int32 expected = 15;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsTotalRowsFromGridSource()
        {
            Int32 actual = new GridPager<GridModel>(grid, null).TotalRows;
            Int32 expected = grid.Source.Count();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsTypeAsPostProcessor()
        {
            GridProcessorType actual = new GridPager<GridModel>(grid, null).Type;
            GridProcessorType expected = GridProcessorType.Post;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsPagesToDisplay()
        {
            Int32 actual = new GridPager<GridModel>(grid, null).PagesToDisplay;
            Int32 expected = 5;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsRowsPerPage()
        {
            Int32 actual = new GridPager<GridModel>(grid, null).RowsPerPage;
            Int32 expected = 20;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GridPager_SetsGrid()
        {
            IGrid actual = new GridPager<GridModel>(grid, null).Grid;
            IGrid expected = grid;

            Assert.AreSame(expected, actual);
        }

        #endregion

        #region Method: Process(IQueryable<TModel> items)

        [Test]
        public void Process_ReturnsPagedItems()
        {
            pager.CurrentPage = 1;
            pager.RowsPerPage = 1;

            IEnumerable<GridModel> expected = grid.Source.Skip(1).Take(1);
            IEnumerable<GridModel> actual = pager.Process(grid.Source);

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region Method: LinkForPage(Int32 page)

        [Test]
        [TestCase("", "", 0)]
        [TestCase("", null, 0)]
        [TestCase("", "MG-Page=", 0)]
        [TestCase("", "MG-Page=0", 0)]
        [TestCase("", "MG-Page=10", 0)]
        [TestCase("", "", 1)]
        [TestCase("", null, 1)]
        [TestCase("", "MG-Page=", 1)]
        [TestCase("", "MG-Page=1", 1)]
        [TestCase("", "MG-Page=10", 1)]
        [TestCase("Grid", "", 0)]
        [TestCase("Grid", null, 0)]
        [TestCase("Grid", "MG-Page-Grid=", 0)]
        [TestCase("Grid", "MG-Page-Grid=0", 0)]
        [TestCase("Grid", "MG-Page-Grid=10", 0)]
        [TestCase("Grid", "", 1)]
        [TestCase("Grid", null, 1)]
        [TestCase("Grid", "MG-Page-Grid=", 1)]
        [TestCase("Grid", "MG-Page-Grid=1", 1)]
        [TestCase("Grid", "MG-Page-Grid=10", 1)]
        public void LinkForPage_GeneratesLinkForPage(String gridName, String queryString, Int32 page)
        {
            RequestContext requestContext = HttpContextFactory.CreateHttpContext(queryString).Request.RequestContext;
            String currentAction = requestContext.RouteData.Values["action"] as String;
            RouteValueDictionary routeValues = new RouteValueDictionary();
            UrlHelper urlHelper = new UrlHelper(requestContext);
            if (!String.IsNullOrWhiteSpace(gridName))
                routeValues["MG-Page-" + gridName] = page;
            else
                routeValues["MG-Page"] = page;
            grid.Name = gridName;

            String actual = new GridPager<GridModel>(grid, requestContext).LinkForPage(page);
            String expected = urlHelper.Action(currentAction, routeValues);

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
