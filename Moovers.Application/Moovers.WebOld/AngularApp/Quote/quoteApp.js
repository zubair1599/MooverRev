/// <reference path="E:\ASP.NET\Moovers\Moovers\Moovers.Application\Moovers.Web\AngularViews/Quote/contacts.html" />
/// <reference path="E:\ASP.NET\Moovers\Moovers\Moovers.Application\Moovers.Web\AngularViews/Quote/contacts.html" />
var quoteApp = angular.module('quoteApp', ['ngRoute', 'ngResource', 'ngAnimate', 'angular-loading-bar', 'ngDragDrop']);



quoteApp.config(['$routeProvider', '$locationProvider',
  function ($routeProvider, $locationProvider) {
      //$locationProvider.html5Mode(true);
      $routeProvider.
          when('/', {
              templateUrl: '/AngularViews/Quote/contacts.html'

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

          }).otherwise({
              redirectTo:'/'

          });
  }]);