app.directive('overflowClass', function () {
    return {
        restrict: 'A',
        replace: false,
        scope: {
        },
        template: '',
        link: function (scope, element) {

            scope.maxWidth = function () {
                let clientWidth = element[0].clientWidth;
                let scrollWidth = element[0].scrollWidth;

                return scrollWidth > clientWidth;
            };
            scope.$watch('maxWidth',
                function (newValue, oldValue) {
                    var val = scope.maxWidth();
                    if (val) {
                        element.removeClass('not_maxwidth');
                    } else {
                        element.addClass('not_maxwidth');
                    }
                });

        }
    }
});