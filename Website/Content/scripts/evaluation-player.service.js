(function () {
	'use strict';

	angular.module('tvbsApp').factory('tvbsEvaluationPlayerService', ['$http', '$q', '$rootScope', '$timeout', EvaluationPlayerService]);

	function EvaluationPlayerService($http, $q, $rootScope, $timeout) {
		var priv = {
			answer: null,
			code: '',
			evaluationId: null,
			isConnecting: false,
			isCorrect: false,
			isSubmitted: false,
			isSubmitting: false,
			isWebSocketOpen: false,
			name: '',
			points: 0,
			question: null,
			questionNumber: 0,
			state: 'connect',
			uid: null,
			webSocket: null
		};

		return {
			connect: connect,
			disconnect: disconnect,
			getAnswer: function () { return priv.answer; },
			getIsConnected: function () { return priv.state !== 'connect'; },
			getIsConnecting: function () { return priv.isConnecting; },
			getIsCorrect: function () { return priv.isCorrect; },
			getIsSubmitted: function () { return priv.isSubmitted; },
			getIsSubmitting: function () { return priv.isSubmitting; },
			getIsWebSocketOpen: function () { return priv.isWebSocketOpen; },
			getName: function () { return priv.name; },
			getPoints: function () { return priv.points; },
			getQuestion: function () { return priv.question; },
			getQuestionNumber: function () { return priv.questionNumber; },
			getState: function () { return priv.state; },
			getUid: function () { return priv.uid; },
			readCookie: readCookie,
			submitAnswer: submitAnswer
		};

		function connect(host, evaluationId, code) {
			priv.code = code;
			priv.evaluationId = evaluationId;
			priv.isConnecting = true;
			priv.isSubmitted = false;

			priv.webSocket = new WebSocket('ws://' + host + '/api/evaluations/websockets');
			priv.webSocket.onclose = onClose;
			priv.webSocket.onerror = onError;
			priv.webSocket.onmessage = onMessage;
			priv.webSocket.onopen = onOpen;

			createCookie('code', priv.code, 7);
		}

		function createCookie(name, value, days) {
			var date = new Date();

			date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));

			document.cookie = name + '=' + value + '; expires=' + date.toUTCString() + '; path=/';
		}

		function disconnect() {
			eraseCookie();

			priv.webSocket.onclose = onClose;
			priv.webSocket.close();

			priv.answer = null;
			priv.code = '';
			priv.evaluationId = null;
			priv.isConnecting = false;
			priv.isCorret = false;
			priv.isSubmitted = false;
			priv.isSubmitting = false;
			priv.isWebSocketOpen = false;
			priv.name = '';
			priv.points = 0;
			priv.question = null;
			priv.questionNumber = 0;
			priv.state = 'connect';
			priv.uid = null;
			priv.webSocket = null;
		}

		function eraseCookie() {
			createCookie('code', '', -1);
		}

		function incrementPoints(points) {
			++priv.points;
			--points;

			if (points > 0) {
				$timeout(function () { incrementPoints(points); }, 10);
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

		function onMessage(event) {
			var json = JSON.parse(event.data);

			switch (json.action) {
				case 'answerSubmitted':
					priv.isSubmitted = true;
					priv.isSubmitting = false;
					break;
				case 'connected':
					priv.answer = null;
					priv.isConnecting = false;
					priv.name = json.data.name;
					priv.points = json.data.points;
					priv.question = json.data.question;
					priv.questionNumber = json.data.questionNumber;
					priv.uid = json.data.uid;

					if (priv.question === null) {
						priv.state = 'intro';
					} else {
						priv.state = 'question';
					}
					break;
				case 'nextQuestionStarted':
					priv.answer = null;
					priv.isCorrect = false;
					priv.isSubmitted = false;
					priv.question = json.data.question;
					priv.questionNumber = json.data.questionNumber;
					priv.state = 'question';

					$rootScope.$broadcast('nextQuestionStarted');
					break;
				case 'notConnected':
					priv.isConnecting = false;
					break;
				case 'restarted':
					priv.question = null;
					priv.questionNumber = 0;
					priv.state = 'intro';
					break;
				case 'showResult':
					for (var i = 0; i < priv.question.options.length; ++i) {
						if (priv.question.options[i].letter === priv.answer && priv.question.options[i].isCorrect) {
							priv.isCorrect = true;
						}
					}
					priv.state = 'result';

					if (priv.isCorrect) {
						incrementPoints(100);
					}
					break;
			}

			$rootScope.$broadcast('webSocketMessaged');
		}

		function onOpen() {
			priv.isWebSocketOpen = true;

			send('connect', { code: priv.code });
		}

		function readCookie() {
			var name = 'code=';
			var values = document.cookie.split(';');
			
			for (var i = 0; i < values.length; ++i) {
				var cookie = values[i];

				while (cookie.charAt(0) == ' ') {
					cookie = cookie.substring(1, cookie.length);
				}

				if (cookie.indexOf(name) == 0) {
					return cookie.substring(name.length, cookie.length);
				}
			}

			return null;
		}

		function send(action, data) {
			var json = {
				action: action,
				data: data,
				sender: 'player'
			};

			priv.webSocket.send(JSON.stringify(json));
		}

		function submitAnswer(answer) {
			priv.answer = answer;
			priv.isSubmitting = true;

			send('submitAnswer', { answer: answer, uid: priv.uid });
		}
	}
})();