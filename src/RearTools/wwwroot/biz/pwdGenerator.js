app.controller('pwd-generator-controller', function ($scope, $http, ngAjax) {
	$scope.model = {
		isLowercaseChars: true,
		lowercaseChars: 'abcdefghijklmnopqrstuvwxyz',
		isUpercaseChars: true,
		upercaseChars: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ',
		isSpecialChars: true,
		specialChars: '!#$^&*()_-=+[]{},.<>?;:`~',
		isNumeric: true,
		numeric: '0123456789',
		pwdLen: 16
	};
	$("body").on("click", "#btnGenerate", function () {

		var condidateChars = "";

		// construct all possible chars
		if ($scope.model.isLowercaseChars) {
			condidateChars += $scope.model.lowercaseChars;
		}

		if ($scope.model.isUpercaseChars) {
			condidateChars += $scope.model.upercaseChars;
		}

		if ($scope.model.isNumeric) {
			condidateChars += $scope.model.numeric;
		}

		if ($scope.model.isSpecialChars) {
			condidateChars += $scope.model.specialChars;
		}

		// post to server to get pwd
		ngAjax.post(window.requestUrls.doGenerate, { candidateChars: condidateChars, len: $scope.model.pwdLen }, function (result) {
			if (result.Code === 0) {
				$scope.result = {
					pwd: result.Data.pwd,
					strength: result.Data.strength,
					maxStrength: result.Data.maxStrength
				}
			}
			else {
				document.getElementById("destChars").value = result.Message;
				$scope.result = {
					pwd: result.Message,
					strength: null,
					maxStrength: 0
				}
			}
		});

	});


})
