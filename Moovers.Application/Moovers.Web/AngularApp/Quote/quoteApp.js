/// <reference path="E:\ASP.NET\Moovers\Moovers\Moovers.Application\Moovers.Web\AngularViews/Quote/contacts.html" />
/// <reference path="E:\ASP.NET\Moovers\Moovers\Moovers.Application\Moovers.Web\AngularViews/Quote/contacts.html" />
var quoteApp = angular.module('quoteApp', ['ngRoute', 'ngResource', 'ngAnimate', 'angular-loading-bar', 'ngDragDrop', 'easypiechart']);



quoteApp.config(['$routeProvider', '$locationProvider','cfpLoadingBarProvider',
  function ($routeProvider, $locationProvider, cfpLoadingBarProvider) {
      //$locationProvider.html5Mode(true);


      cfpLoadingBarProvider.includeBar = true;
      cfpLoadingBarProvider.includeSpinner = true;
      cfpLoadingBarProvider.latencyThreshold = 100;

      $routeProvider.
          when('/', {
              templateUrl: '/AngularViews/Quote/contacts.html'

          }).when('/dashboard', {
              templateUrl: '/AngularViews/Quote/dashboard.html'

          }).when('/accounts', {
              templateUrl: '/AngularViews/Quote/accounts.html'

          }).when('/newquote', {
              templateUrl: '/AngularViews/Quote/NewQuote.html'

          }).when('/stops', {
              templateUrl: '/AngularViews/Quote/stops.html',
              
              name: 'stop view'
          }).when('/inventory', {
              templateUrl: '/AngularViews/Quote/inventory.html',
              name: 'inventory view'
          }).when('/contacts', {
              templateUrl: '/AngularViews/Quote/contacts.html'
              
          }).when('/pricing', {
              templateUrl: '/AngularViews/Quote/pricing.html'

          }).when('/schedule', {
              templateUrl: '/AngularViews/Quote/schedule.html'

          }).when('/home', {
              templateUrl: '/AngularViews/Quote/home.html'

          }).when('/overview', {
              templateUrl: '/AngularViews/Quote/overview.html'

          }).otherwise({
              redirectTo:'/'

          });
      //$locationProvider.html5Mode(true).hashPrefix('!');
  }]);