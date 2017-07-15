(function () {
	'use strict';

	angular.module('tvbsApp').factory('tvbsEvaluationPresenterService', ['$http', '$q', '$rootScope', '$timeout', EvaluationPresenterService]);

	function EvaluationPresenterService($http, $q, $rootScope, $timeout) {
		var priv = {
			evaluationId: null,
			isWebSocketOpen: false,
			state: 'players',
			uid: null,
			webSocket: null
		};

		return {
			connect: connect,
			getIsWebSocketOpen: function () { return priv.isWebSocketOpen; },
			getState: function () { return priv.state; },
			getUid: function () { return priv.uid; },
			restart: function () { send('restart', null); },
			showResults: showResults,
			startNextQuestion: startNextQuestion
		};


		function connect(host, evaluationId) {
			priv.evaluationId = evaluationId;

			priv.webSocket = new WebSocket('ws://'+ host + '/api/evaluations/websockets');
			priv.webSocket.onclose = onClose;
			priv.webSocket.onerror = onError;
			priv.webSocket.onmessage = onMessage;
			priv.webSocket.onopen = onOpen;
		}

		function onClose(event) {
			console.log('WebSocket Closed');
			console.log(event);

			priv.isWebSocketOpen = false;
			$rootScope.$broadcast('webSocketMessaged');
		}

		function onError(event) {
			console.log('WebSocket Error');
			console.log(event);
		}

		function onOpen() {
			priv.isWebSocketOpen = true;

			send('connect', null);
		}

		function onMessage(event) {
			var i;
			var json = JSON.parse(event.data);

			switch (json.action) {
				case 'connected':
					priv.state = json.data.state;
					priv.uid = json.data.uid;
					break;
				case 'nextQuestionStarted':
					priv.state = 'question';
					break;
				case 'restarted':
					priv.state = 'players';
					break;
			}

			$rootScope.$broadcast('webSocketMessaged');
		}

		function send(action, data) {
			var json = {
				action: action,
				data: data,
				sender: 'presenter'
			};

			priv.webSocket.send(JSON.stringify(json));
		}

		function showResults() {
			send('showResults', null);

			priv.state = 'players';
		}

		function startNextQuestion() {
			send('startNextQuestion', null);
		}
	}
})();