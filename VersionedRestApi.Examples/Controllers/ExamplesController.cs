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
    public class ExamplesController : ApiController
    {
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
            return "This method is consistent for all version of the API";
        }

        //this method handles GET requests to "/v1/Examples/"
        [ApiRoute("Examples/", AcceptedVersions = new [] {1} )]
        [HttpGet]
        public string Get()
        {
            return "This was created for version 1 of the API";
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
        public string SomeBreakingChangeToGet()
        {
            return "This was a breaking change that caused the API to need to be versioned to version 2. This action didn't change again until version 7 (see below).";
        }

        /*
         * This method handles requests to every version  
         */
        [ApiRoute("Examples/")]/** TODO add support for a start version that goes through current version*/
        [HttpGet]
        public string YetAnotherBreakingChangeToGet()
        {
            return "This was a breaking change that caused the API to need to be versioned to version 7.";
        }
    }
}
