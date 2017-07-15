(function () {
	'use strict';

	angular.module('tvbsApp').component('tvbsEvaluationPresenter', {
		bindings: {
			color: '@',
			evaluationId: '<',
			host: '@'
		},
		controller: ['$scope', '$timeout', 'tvbsEvaluationPresenterService', EvaluationPresenterController],
		controllerAs: 'vm',
		templateUrl: '/content/scripts/evaluation-presenter.component.html'
	});

	function EvaluationPresenterController($scope, $timeout, tvbsEvaluationPresenterService) {
		var vm = this;

		// Public Methods
		vm.$postLink = function () { tvbsEvaluationPresenterService.connect(vm.host, vm.evaluationId); };
		vm.getIsWebSocketOpen = function () { return tvbsEvaluationPresenterService.getIsWebSocketOpen(); };
		vm.getState = function () { return tvbsEvaluationPresenterService.getState(); };
		vm.getUid = function () { return tvbsEvaluationPresenterService.getUid(); };
		vm.reset = function (id) { window.location.href = '/reset/' + id; };
		vm.restart = function () { tvbsEvaluationPresenterService.restart(); };
		vm.showResults = function () { tvbsEvaluationPresenterService.showResults(); };
		vm.startNextQuestion = function () { tvbsEvaluationPresenterService.startNextQuestion(); };

		// Events
		$scope.$on('webSocketMessaged', function (event, data) { applyScopeManually($scope); });
	}
})();