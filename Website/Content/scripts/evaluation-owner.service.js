(function () {
	'use strict';

	angular.module('tvbsApp').factory('tvbsEvaluationOwnerService', ['$http', '$q', '$rootScope', '$timeout', EvaluationOwnerService]);

	function EvaluationOwnerService($http, $q, $rootScope, $timeout) {
		var priv = {
			evaluationId: null,
			isWebSocketOpen: false,
			players: [],
			question: null,
			questionNumber: 0,
			state: 'players',
			uid: null,
			webSocket: null
		};

		return {
			connect: connect,
			getIsWebSocketOpen: function () { return priv.isWebSocketOpen; },
			getPlayers: function () { return priv.players; },
			getQuestion: function () { return priv.question; },
			getQuestionNumber: function () { return priv.questionNumber; },
			getState: function () { return priv.state; },
			getUid: function () { return priv.uid; },
			restart: function () { send('restart', null); }
		};

		
		function connect(host, evaluationId) {
			priv.evaluationId = evaluationId;

			priv.webSocket = new WebSocket('ws://'+ host + '/api/evaluations/websockets');
			priv.webSocket.onclose = onClose;
			priv.webSocket.onerror = onError;
			priv.webSocket.onmessage = onMessage;
			priv.webSocket.onopen = onOpen;
		}
		
		function incrementPoints(i, points) {
			var temp;

			if (points > 1000) {
				temp = 100;
			} else if (points > 100) {
				temp = 10;
			} else {
				temp = 1;
			}

			priv.players[i].points += temp;
			points -= temp;

			if (points > 0) {
				$timeout(function () { incrementPoints(i, points); }, 10);
			}
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
				case 'answerSubmitted':
					for (i = 0; i < priv.players.length; ++i) {
						if (priv.players[i].uid === json.data.playerUid) {
							priv.players[i].answer = { };
							priv.players[i].answer.isCorrect = json.data.answer.isCorrect;
							priv.players[i].answer.letter = json.data.answer.letter;
						}
					}
					break;
				case 'connected':
					priv.players = json.data.players;
					priv.question = json.data.question;
					priv.questionNumber = json.data.questionNumber;
					priv.uid = json.data.uid;
					break;
				case 'nextQuestionStarted':
					for (i = 0; i < priv.players.length; ++i) {
						priv.players[i].answer = {
							isCorrect: null,
							letter: ''
						};
					}

					priv.isAnsweringQuestion = true;
					priv.question = json.data.question;
					priv.questionNumber = json.data.questionNumber;
					priv.state = 'question';
					break;
				case 'playerConnected':
					for (i = 0; i < priv.players.length; ++i) {
						if (priv.players[i].uid === json.data.player.uid) {
							priv.players[i].isConnected = true;
						}
					}
					break;
				case 'playerDisconnected':
					for (i = 0; i < priv.players.length; ++i) {
						if (priv.players[i].uid === json.data.player.uid) {
							priv.players[i].isConnected = false;
						}
					}
					break;
				case 'restarted':
					priv.isAnsweringQuestion = false;
					priv.question = null;
					priv.questionNumber = 0;
					priv.state = 'players';

					for (i = 0; i < priv.players.length; ++i) {
						priv.players[i].points = 0;
					}
					break;
				case 'showResults':
					priv.state = 'players';

					for (i = 0; i < priv.players.length; ++i) {
						if (priv.players[i].answer.isCorrect) {
							incrementPoints(i, 100);
						}
					}

					priv.isAnsweringQuestion = false;
					break;
				case 'showTotals':
					console.log(priv.players);
					for (i = 0; i < priv.players.length; ++i) {
						incrementPoints(i, priv.players[i].totalPoints);
					}
					break;
			}

			$rootScope.$broadcast('webSocketMessaged');
		}

		function send(action, data) {
			var json = {
				action: action,
				data: data,
				sender: 'owner'
			};

			priv.webSocket.send(JSON.stringify(json));
		}
	}
})();