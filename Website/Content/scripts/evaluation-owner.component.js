(function () {
	'use strict';

	angular.module('tvbsApp').component('tvbsEvaluationOwner', {
		bindings: {
			color: '@',
			evaluationId: '<',
			host: '@',
			team: '@'
		},
		controller: ['$scope', '$timeout', 'tvbsEvaluationOwnerService', EvaluationOwnerController],
		controllerAs: 'vm',
		templateUrl: '/content/scripts/evaluation-owner.component.html'
	});

	function EvaluationOwnerController($scope, $timeout, tvbsEvaluationOwnerService) {
		var vm = this;

		// Public Methods
		vm.$postLink = function () { $timeout(function () { tvbsEvaluationOwnerService.connect(vm.host, vm.evaluationId); }); };
		vm.acbdComparator = acbdComparator;
		vm.getIsWebSocketOpen = function () { return tvbsEvaluationOwnerService.getIsWebSocketOpen(); };
		vm.getPlayers = function () { return tvbsEvaluationOwnerService.getPlayers(); };
		vm.getQuestion = function () { return tvbsEvaluationOwnerService.getQuestion(); };
		vm.getQuestionNumber = function () { return tvbsEvaluationOwnerService.getQuestionNumber(); };
		vm.getState = function () { return tvbsEvaluationOwnerService.getState(); };
		vm.getUid = function () { return tvbsEvaluationOwnerService.getUid(); };
		vm.hasPlayerAnswered = function (player) { return angular.isDefined(player.answer) && angular.isDefined(player.answer.letter) && player.answer.letter !== ''; };
		vm.hasQuestion = function () { return tvbsEvaluationOwnerService.getQuestion() !== null; };
		vm.restart = function () { tvbsEvaluationOwnerService.restart(); };
		vm.showResults = function () { tvbsEvaluationOwnerService.showResults(); };
		vm.startNextQuestion = function () { tvbsEvaluationOwnerService.startNextQuestion(); };

		// Events
		$scope.$on('webSocketMessaged', function (event, data) { applyScopeManually($scope); });


		function acbdComparator(v1, v2) {
			switch (v1.value.toLowerCase()) {
				case 'a':
					return -1;
				case 'b':
					return v2.value.toLowerCase() === 'a' || v2.value.toLowerCase() === 'c' ? 1 : -1;
				case 'c':
					return v2.value.toLowerCase() === 'a' ? 1 : -1;
				case 'd':
					return 1;
			}
		}
	}
})();