/**
 * Cube - Bootstrap Admin Theme
 * Copyright 2014 Phoonio
 */

var app = angular.module('cubeWebApp', [
	'ngRoute',
	'angular-loading-bar',
	'ngAnimate',
	'easypiechart'
]);

app.config(['cfpLoadingBarProvider', function(cfpLoadingBarProvider) {
	cfpLoadingBarProvider.includeBar = true;
	cfpLoadingBarProvider.includeSpinner = true;
	cfpLoadingBarProvider.latencyThreshold = 100;
}]);

/**
 * Configure the Routes
 */
app.config(['$routeProvider', function ($routeProvider) {
	$routeProvider
		.when("/", {
			redirectTo:'/dasboard'
		})
		.when("/dasboard", {
			templateUrl: "views/dashboard.html", 
			controller: "mainCtrl", 
			title: 'Dashboard'
		})
		.when("/tables/simple", {
			templateUrl: "views/tables.html", 
			controller: "mainCtrl", 
			title: 'Tables'
		})
		.when("/tables/tables-advanced", {
			templateUrl: "views/tables-advanced.html", 
			controller: "mainCtrl", 
			title: 'Advanced tables'
		})
		.when("/tables/users", {
			templateUrl: "views/users.html", 
			controller: "mainCtrl", 
			title: 'Users'
		})

		.when("/graphs/graphs-xcharts", {
			templateUrl: "views/graphs-xcharts.html", 
			controller: "mainCtrl", 
			title: 'XCharts'
		})
		.when("/graphs/graphs-morris", {
			templateUrl: "views/graphs-morris.html", 
			controller: "mainCtrl", 
			title: 'Morris charts'
		})
		.when("/graphs/graphs-flot", {
			templateUrl: "views/graphs-flot.html", 
			controller: "mainCtrl", 
			title: 'Flot charts'
		})
		.when("/graphs/graphs-dygraphs", {
			templateUrl: "views/graphs-dygraphs.html", 
			controller: "mainCtrl", 
			title: 'Dygraphs'
		})

		.when("/email/email-compose", {
			templateUrl: "views/email-compose.html", 
			controller: "emailCtrl", 
			title: 'Email compose'
		})
		.when("/email/email-inbox", {
			templateUrl: "views/email-inbox.html", 
			controller: "emailCtrl", 
			title: 'Email inbox'
		})
		.when("/email/email-detail", {
			templateUrl: "views/email-detail.html", 
			controller: "emailCtrl", 
			title: 'Email detail'
		})

		.when("/widgets", {
			templateUrl: "views/widgets.html", 
			controller: "mainCtrl", 
			title: 'Widgets'
		})

		.when("/pages/user-profile", {
			templateUrl: "views/user-profile.html", 
			controller: "mainCtrl", 
			title: 'User profile'
		})
		.when("/pages/calendar", {
			templateUrl: "views/calendar.html", 
			controller: "mainCtrl", 
			title: 'Calendar'
		})
		.when("/pages/timeline", {
			templateUrl: "views/timeline.html", 
			controller: "mainCtrl", 
			title: 'Timeline'
		})
		.when("/pages/timeline-grid", {
			templateUrl: "views/timeline-grid.html", 
			controller: "mainCtrl", 
			title: 'Timeline grid'
		})
		.when("/pages/team-members", {
			templateUrl: "views/team-members.html", 
			controller: "mainCtrl", 
			title: 'Team members'
		})
		.when("/pages/search", {
			templateUrl: "views/search.html", 
			controller: "mainCtrl", 
			title: 'Search results'
		})
		.when("/pages/projects", {
			templateUrl: "views/projects.html", 
			controller: "mainCtrl", 
			title: 'Projects'
		})
		.when("/pages/pricing", {
			templateUrl: "views/pricing.html", 
			controller: "mainCtrl", 
			title: 'Pricing'
		})
		.when("/pages/invoice", {
			templateUrl: "views/invoice.html", 
			controller: "mainCtrl", 
			title: 'Invoice'
		})
		.when("/pages/intro", {
			templateUrl: "views/intro.html", 
			controller: "mainCtrl", 
			title: 'Tour layout'
		})
		.when("/pages/gallery", {
			templateUrl: "views/gallery.html", 
			controller: "mainCtrl", 
			title: 'Gallery'
		})
		.when("/pages/gallery-v2", {
			templateUrl: "views/gallery-v2.html", 
			controller: "mainCtrl", 
			title: 'Gallery v2'
		})

		.when("/forms/x-editable", {
			templateUrl: "views/x-editable.html", 
			controller: "mainCtrl", 
			title: 'X-Editable'
		})
		.when("/forms/form-elements", {
			templateUrl: "views/form-elements.html", 
			controller: "mainCtrl", 
			title: 'Form elements'
		})
		.when("/forms/form-ckeditor", {
			templateUrl: "views/form-ckeditor.html", 
			controller: "mainCtrl", 
			title: 'Ckeditor'
		})
		.when("/forms/form-wysiwyg", {
			templateUrl: "views/form-wysiwyg.html", 
			controller: "mainCtrl", 
			title: 'Wysiwyg'
		})
		.when("/forms/form-wizard", {
			templateUrl: "views/form-wizard.html", 
			controller: "mainCtrl", 
			title: 'Wizard'
		})
		.when("/forms/form-wizard-popup", {
			templateUrl: "views/form-wizard-popup.html", 
			controller: "mainCtrl", 
			title: 'Wizard popup'
		})
		.when("/forms/form-dropzone", {
			templateUrl: "views/form-dropzone.html", 
			controller: "mainCtrl", 
			title: 'Dropzone'
		})
		.when("/forms/form-summernote", {
			templateUrl: "views/form-summernote.html", 
			controller: "mainCtrl", 
			title: 'Wysiwyg Summernote'
		})

		.when("/ui-kit/ui-elements", {
			templateUrl: "views/ui-elements.html", 
			controller: "mainCtrl", 
			title: 'UI elements'
		})
		.when("/ui-kit/ui-nestable", {
			templateUrl: "views/ui-nestable.html", 
			controller: "mainCtrl", 
			title: 'UI nestable'
		})
		.when("/ui-kit/video", {
			templateUrl: "views/video.html", 
			controller: "mainCtrl", 
			title: 'Video'
		})
		.when("/ui-kit/typography", {
			templateUrl: "views/typography.html", 
			controller: "mainCtrl", 
			title: 'Typography'
		})
		.when("/ui-kit/notifications", {
			templateUrl: "views/notifications.html", 
			controller: "mainCtrl", 
			title: 'Notifications and Alerts'
		})
		.when("/ui-kit/modals", {
			templateUrl: "views/modals.html", 
			controller: "mainCtrl", 
			title: 'Modals'
		})
		.when("/ui-kit/icons/icons-awesome", {
			templateUrl: "views/icons-awesome.html", 
			controller: "mainCtrl", 
			title: 'Awesome icons'
		})
		.when("/ui-kit/icons/icons-halflings", {
			templateUrl: "views/icons-halflings.html", 
			controller: "mainCtrl", 
			title: 'Halflings icons'
		})

		.when("/google-maps", {
			templateUrl: "views/maps.html", 
			controller: "mainCtrl", 
			title: 'Google maps'
		})

		.when("/extra/faq", {
			templateUrl: "views/faq.html", 
			controller: "mainCtrl", 
			title: 'FAQ'
		})
		.when("/extra/extra-grid", {
			templateUrl: "views/extra-grid.html", 
			controller: "mainCtrl", 
			title: 'Extra grid'
		})
		.when("/extra/email-templates", {
			templateUrl: "views/emails.html", 
			controller: "mainCtrl", 
			title: 'Email templates'
		})

		.when("/error-404-v2", {
			templateUrl: "views/error-404-v2.html", 
			controller: "mainCtrl",
			title: 'Error 404'
		})
		.when("/error-404", {
			templateUrl: "views/error-404-v2.html", 
			controller: "mainCtrl",
			title: 'Error 404'
		})
		.otherwise({
			redirectTo:'/error-404'
		});
}]);

app.run(['$location', '$rootScope', function($location, $rootScope) {
    $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
        $rootScope.title = current.$$route.title;
    });
}]);