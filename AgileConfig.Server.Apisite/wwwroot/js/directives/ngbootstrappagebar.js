(function () {
    'use strict';
    var directive = function () {
        var htmlTemplate="<nav ng-show='pageInfo.totalPages>0&&pageInfo.showPages>0'><ul class='pagination'><li><a href='javascript:void(0)' ng-click='prePages()' ng-show='showPrePageBtn'>&laquo;</a></li>"+
            "<li ng-repeat='p in currentPages' ng-class='activeClass(p.index)'><a href='javascript:void(0)' ng-click='goPage(p.index)'>{{p.index}}</a></li>"+
            "<li><a href='javascript:void(0)' ng-click='nextPages()' ng-show='showNextPageBtn'>&raquo;</a></li></ul></nav>";
        return {
            restrict: 'EA',    
            scope: {
                pageInfo: '=',
                onChange:'&'         
            },
            template: htmlTemplate,
            link: function (scope, element, attrs) { 
                scope.showPrePageBtn=false;
                scope.showNextPageBtn=false;
                scope.currentPages=[];
                scope.pageInfo.showPages = scope.pageInfo.showPages?scope.pageInfo.showPages:5;
                scope.pageInfo.pageIndex = scope.pageInfo.pageIndex?scope.pageInfo.pageIndex:1;
                scope.pageInfo.totalPages= scope.pageInfo.totalPages?scope.pageInfo.totalPages:0;

                scope.$watch('pageInfo.totalPages',function(newValue,oldValue){
                    if(newValue&&newValue!=oldValue){
                        // if totalPages changed to rebuild the directive
                        build();
                    }
                });
                scope.$watch('pageInfo.showPages',function(newValue,oldValue){
                    if(newValue&&newValue!=oldValue){
                        // if showPages changed to rebuild the directive
                        build();
                    }
                });
                scope.$watch('pageInfo.pageIndex',function(newValue,oldValue){
                    if(newValue&&newValue!=oldValue){
                        // if pageIndex changed to rebuild the directive
                        for(var i in scope.currentPages){
                            var index = scope.currentPages[i].index;
                            if(index ===  scope.pageInfo.pageIndex ){
                                //if pageIndex in showing pages not rebuild.
                                return;
                            }
                        }
                        build();
                    }
                });

                var setPrepageOrNextpageBtnStatus=function(){
                    if( scope.currentPages.length>0){
                        scope.showPrePageBtn = !(scope.currentPages[0].index === 1);
                        scope.showNextPageBtn = !(scope.currentPages[scope.currentPages.length-1].index === scope.pageInfo.totalPages);
                    }
                }

                var build=function(){
                    console.log('build ngbootstrappagebar .');
                    scope.currentPages=[];
                    if(scope.pageInfo.pageIndex > scope.pageInfo.totalPages){
                        scope.pageInfo.pageIndex = scope.pageInfo.totalPages;
                    }
                    if(scope.pageInfo.totalPages>0&&scope.pageInfo.pageIndex==0){
                        scope.pageInfo.pageIndex = 1;
                    }
                    var y =scope.pageInfo.pageIndex%scope.pageInfo.showPages;
                    if(y===0){
                        y=5;
                    }
                        
                    for(var i=0;i<scope.pageInfo.showPages;i++){
                        var index = scope.pageInfo.pageIndex-y+1+i;
                        if(index>0&&index<= scope.pageInfo.totalPages){
                            scope.currentPages.push({
                                index:scope.pageInfo.pageIndex-y+1+i
                            });
                        }
                    }
                        
                    setPrepageOrNextpageBtnStatus();
                }

                 

                scope.activeClass=function(index){
                    if(index===scope.pageInfo.pageIndex){
                        return 'active';
                    }else{
                        return '';
                    }
                };
                scope.goPage=function(index){
                    console.log('go to page :',index);                        
                    scope.pageInfo.pageIndex = index;
                      
                    if(scope.onChange()){
                        scope.onChange()(index);
                    }
                };

                scope.nextPages=function(){
                    if(scope.currentPages.length > 0){
                        var lastPageIndex = scope.currentPages[scope.currentPages.length-1].index;
                        if(lastPageIndex === scope.pageInfo.totalPages){
                            return;
                        }
                        scope.currentPages = [];
                        for(var i =0;i< scope.pageInfo.showPages;i++){
                            var index = lastPageIndex + i + 1 ;
                            if(index <= scope.pageInfo.totalPages){
                                scope.currentPages.push({index:index});
                            }
                               
                        }
                        setPrepageOrNextpageBtnStatus();
                    }
                }
                    
                scope.prePages=function(){
                    if(scope.currentPages.length > 0){
                        var firstPageIndex = scope.currentPages[0].index;
                        if(firstPageIndex === 1){
                            return;
                        }
                        scope.currentPages = [];
                        for(var i = scope.pageInfo.showPages ; i > 0 ; i--){
                            var index = firstPageIndex - i ;
                            if(index > 0 ){
                                scope.currentPages.push({index:index});
                            }
                               
                        }
                        setPrepageOrNextpageBtnStatus();
                    }
                }

                build();
            } 
        };
    };

    angular.module('agile.bootstrap-pagebar',[]).directive('pagebar', directive);

}());