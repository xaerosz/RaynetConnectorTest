using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RaynetConnectorTest.Models;
using System;

namespace RaynetConnectorTest.Pages
{
    public class TestRaynetApiCallModel : PageModel
    {
        private readonly IRaynetConnector _raynetConnector;
        public TestRaynetApiCallModel(IRaynetConnector raynetConnector)
        {
            _raynetConnector = raynetConnector;
        }

        public void OnGet()
        {
            var raynetCrmBusinessCasesResult = _raynetConnector.GetAllBusinessCases(null).Result;
            ViewData["data"] = raynetCrmBusinessCasesResult.result;
        }
    }
}
