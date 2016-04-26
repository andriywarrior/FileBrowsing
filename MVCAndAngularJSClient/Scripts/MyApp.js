(function () {
    var app = angular.module('MyApp', []);

    app.controller('HomeController', function ($scope, $http) {
        $http.get('http://localhost:1035/api/Data/GetModel').success(function (response) {
            $scope.result = response;
        });

        $scope.getModel = function (name, path) {
            var url = 'http://localhost:1035/api/Data/GetModel' + '?name=' + name;

            if (path != '')
                url += '&path=' + path;

            $http.get(url).success(function (response) {
                if (response.Error == null)
                    $scope.result = response;
                else
                    alert(response.Error);
            });
        };
    });
})();