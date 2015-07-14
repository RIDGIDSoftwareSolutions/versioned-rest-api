﻿#region LICENSE
// The MIT License (MIT)
// 
// Copyright (c) 2015 Jake Gordon
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using System;
using System.Configuration;
using System.Configuration.Abstractions;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace VersionedRestApi.Tests
{
    [TestFixture]
    public class ApiRouteAttributeTests
    {
        [Test]
        public void ConstructorThrowsAnArgumentNullExceptionIfTheTemplateIsNull()
        {
            ArgumentNullException expectedException = new ArgumentNullException("template");
            Exception exception = Assert.Throws<ArgumentNullException>(() => new ApiRouteAttribute(null));

            Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
        }

        [Test]
        public void ConstructorThrowsAnArgumentNullExceptionIfTheTemplateIsWhitespace()
        {
            ArgumentNullException expectedException = new ArgumentNullException("template");
            Exception exception = Assert.Throws<ArgumentNullException>(() => new ApiRouteAttribute("   "));

            Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
        }

        [Test]
        public void ConstructorThrowsAnArgumentExceptionIfTheTemplateStartsWithAForwardSlash()
        {
            Exception exception = Assert.Throws<ArgumentException>(() => new ApiRouteAttribute("/GamingGroups"));

            Assert.That(exception.Message, Is.EqualTo("The route cannot start with a forward slash ('/') since it will be prefixed with the api version (e.g. api/v2/)."));
        }

        [Test]
        public void BuildRouteThrowsAConfigurationErrorsExceptionIfTheCurrentApiVersionAppSettingIsNotAPositiveInteger()
        {
            const string CURRENT_VERSION = "-1";
            var configurationManager = MockCurrentApiVersionAs(CURRENT_VERSION);

            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager
            };

            var exception = Assert.Throws<ConfigurationErrorsException>(() => routeAttribute.BuildRoute());

            Assert.That(exception.Message, Is.EqualTo("The 'currentApiVersion' app setting must be a positive integer."));
        }

        [Test]
        public void BuildRouteMakesARouteSupportingAllVersionsUpToTheCurrentVersion()
        {
            const string CURRENT_VERSION = "3";
            var configurationManager = MockCurrentApiVersionAs(CURRENT_VERSION);
            
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager
            };

            string actualRoute = routeAttribute.BuildRoute();

            Assert.That(actualRoute, Is.EqualTo("api/v{version:int:regex(1|2|3)}/GamingGroups"));
        }

        [Test]
        public void BuildRouteMakesARouteSupportingTheSpecifiedVersions()
        {
            var configurationManager = MockCurrentApiVersionAs("50");
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager,
                AcceptedVersions = new []{2,3,4}
            };

            string actualRoute = routeAttribute.BuildRoute();

            Assert.That(actualRoute, Is.EqualTo("api/v{version:int:regex(2|3|4)}/GamingGroups"));
        }

        [Test]
        public void BuildRouteThrowsAnInvalidOperationExceptionIfOneOfTheExplicitlySetAcceptedVersionsIsNotAPositiveInteger()
        {
            var configurationManager = MockCurrentApiVersionAs("50");
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager,
                AcceptedVersions = new[] { 2, 3, -4 }
            };

            var exception = Assert.Throws<InvalidOperationException>(() => routeAttribute.BuildRoute());

            Assert.That(exception.Message, Is.EqualTo("The explicitly specified AcceptedVersion values must all be positive integers."));
        }

        [Test]
        public void BuildRouteMakesARouteSupportingAllVersionsFromAStartingVersionToTheCurrentVersion()
        {
            const string CURRENT_VERSION = "4";
            var configurationManager = MockCurrentApiVersionAs(CURRENT_VERSION);
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager,
                StartingVersion = 2
            };

            string actualRoute = routeAttribute.BuildRoute();

            Assert.That(actualRoute, Is.EqualTo("api/v{version:int:regex(2|3|4)}/GamingGroups"));
        }

        [Test]
        public void BuildRouteThrowsAnInvalidOperationExceptionIfBothStartingVersionAndAcceptedVersionsAreSet()
        {
            var configurationManager = MockCurrentApiVersionAs("50");
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager,
                AcceptedVersions = new[] { 2 },
                StartingVersion = 1
            };

            var exception = Assert.Throws<InvalidOperationException>(() => routeAttribute.BuildRoute());

            Assert.That(exception.Message, Is.EqualTo("Either 'AcceptedVersions' or 'StartingVersion' can be set, but not both."));
        }

        [Test]
        public void BuildRouteThrowsAnInvalidOperationExceptionIfTheStartingVersionIsGreaterThanTheCurrentApiversion()
        {
            var configurationManager = MockCurrentApiVersionAs("1");
            ApiRouteAttribute routeAttribute = new ApiRouteAttribute("GamingGroups")
            {
                ConfigurationManager = configurationManager,
                StartingVersion = 2
            };

            var exception = Assert.Throws<InvalidOperationException>(() => routeAttribute.BuildRoute());

            Assert.That(exception.Message, Is.EqualTo("The 'StartingVersion' cannot be greater than the 'currentApiVersion' specified in the config."));
        }

        private static IConfigurationManager MockCurrentApiVersionAs(string currentVersion)
        {
            IConfigurationManager configurationManager = MockRepository.GenerateMock<IConfigurationManager>();
            IAppSettings appSettings = MockRepository.GenerateMock<IAppSettings>();
            appSettings.Expect(mock => mock.AppSetting(ApiRouteAttribute.APP_KEY_CURRENT_API_VERSION)).Return(currentVersion);
            configurationManager.Expect(mock => mock.AppSettings).Return(appSettings);
            return configurationManager;
        }
    }
}
