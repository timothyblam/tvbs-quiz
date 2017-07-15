(function () {
	'use strict';
	angular.module('tvbsApp').component('tvbsCalloutError', {
		bindings: {
			response: '<'
		},
		controller: CalloutErrorController,
		controllerAs: 'vm',
		templateUrl: '/api/content/callout-error.component.html'
	});

	function CalloutErrorController() {
		var vm = this;

		// Public Methods
		vm.hasError = function () { return vm.response !== null; };
	}
})();