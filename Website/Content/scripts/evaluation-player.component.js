(function () {
	'use strict';

	angular.module('tvbsApp').component('tvbsEvaluationPlayer', {
		bindings: {
			color: '@',
			evaluationId: '<',
			host: '@'
		},
		controller: ['$rootScope', '$scope', '$timeout', 'tvbsEvaluationPlayerService', EvaluationPlayerController],
		controllerAs: 'vm',
		templateUrl: '/content/scripts/evaluation-player.component.html'
	});

	function EvaluationPlayerController($rootScope, $scope, $timeout, tvbsEvaluationPlayerService) {
		var vm = this;

		// Public Variables
		vm.answer = null;
		vm.code = '';
		vm.connectionAttempted = false;
		vm.timeout = null;

		// Public Methods
		vm.$postLink = postLink;
		vm.connect = function () { vm.connectionAttempted = true; tvbsEvaluationPlayerService.connect(vm.host, vm.evaluationId, vm.code); };
		vm.getAnswer = function () { return tvbsEvaluationPlayerService.getAnswer(); };
		vm.getIsConnecting = function () { return tvbsEvaluationPlayerService.getIsConnecting(); };
		vm.getIsCorrect = function () { return tvbsEvaluationPlayerService.getIsCorrect(); };
		vm.getIsConnected = function () { return tvbsEvaluationPlayerService.getIsConnected(); };
		vm.getIsSubmitted = function () { return tvbsEvaluationPlayerService.getIsSubmitted(); };
		vm.getIsSubmitting = function () { return tvbsEvaluationPlayerService.getIsSubmitting(); };
		vm.getIsWebSocketOpen = function () { return tvbsEvaluationPlayerService.getIsWebSocketOpen(); };
		vm.getName = function () { return tvbsEvaluationPlayerService.getName(); };
		vm.getPoints = function () { return tvbsEvaluationPlayerService.getPoints(); };
		vm.getQuestion = function () { return tvbsEvaluationPlayerService.getQuestion(); };
		vm.getQuestionNumber = function () { return tvbsEvaluationPlayerService.getQuestionNumber(); };
		vm.getState = function () { return tvbsEvaluationPlayerService.getState(); };
		vm.getUid = function () { return tvbsEvaluationPlayerService.getUid(); };
		vm.logOff = logOff;
		vm.submitAnswer = function () { tvbsEvaluationPlayerService.submitAnswer(vm.answer); };

		// Events
		$rootScope.$on('nextQuestionStarted', function () { vm.answer = null; });
		$scope.$on('webSocketMessaged', function (event, data) { applyScopeManually($scope); });


		function logOff() {
			vm.code = '';
			vm.connectionAttempted = false;

			tvbsEvaluationPlayerService.disconnect();
		}

		function keepAlive() {
			console.log('keep alive');

			vm.timeout = $timeout(function () {
				if (vm.getIsConnected()) {
					if (!vm.getIsWebSocketOpen()) {
						console.log('closed');

						vm.answer = null;

						vm.connect();
					}
				}

				keepAlive();
			}, 1000);
		}

		function postLink() {
			var code = tvbsEvaluationPlayerService.readCookie();

			if (code !== null && code !== '') {
				vm.code = code;

				vm.connect();
			}

			keepAlive();
		}
	}
})();