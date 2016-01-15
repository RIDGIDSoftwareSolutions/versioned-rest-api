using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;

namespace VersionedRestApi.Examples.Controllers
{
    public class ExamplesApiController : ApiController
    {
        [HttpGet]
        public string HelloWorld()
        {
            return "hello world";
        }

        /* this method handles POST requests to version 1 all the way through the current version of the API (as specified by
         * the currentApiVersion app setting) since no versions are explicitly specified. 
         * For example, if the currentApiVersion == 8, then this action will respond to
         * all of these:
            /v1/Examples/
            /v2/Examples/
            /v3/Examples/
            /v4/Examples/
            /v5/Examples/
            /v6/Examples/
            /v7/Examples/
            /v8/Examples/
         */
        [ApiRoute("Examples/")]
        [HttpPost]
        public string Post()
        {
            return "Return value for the POST method.";
        }

        //this method handles GET requests to "/v1/Examples/"
        [ApiRoute("Examples/", AcceptedVersions = new [] {1} )]
        [HttpGet]
        public string Get()
        {
            return "Return value for GET /v1/Examples/";
        }

        /*
         this method handles GET requests to any of the following: 
            /v2/Examples/
            /v3/Examples/
            /v4/Examples/
            /v5/Examples/
            /v6/Examples/
        */
        [ApiRoute("Examples/", AcceptedVersions = new[] {2, 3, 4, 5, 6} )]
        [HttpGet]
        public string SomeBreakingChangeToTheGetAction()
        {
            return "Return value for GET /v(2|3|4|5|6)/Examples/";
        }

        /*
         * This method handles requests to version 7 and up (e.g. /v7/Examples/)  
         */
        [ApiRoute("Examples/", AcceptedVersions = new[] { 7, 8, 9, 10, 11, 12 })]
        [HttpGet]
        public string YetAnotherBreakingChangeToGet()
        {
            return "Return value for GET /v(7|8|9|10|11|12)/Examples/";
        }

        [ApiRoute("Examples/", StartingVersion = 13)]
        [HttpGet]
        public string LatestBreakingChangeToGet()
        {
            return "Return value for GET /v(13|14})/Examples/ ; assuming the current version is at 14.";
        }
    }
}
