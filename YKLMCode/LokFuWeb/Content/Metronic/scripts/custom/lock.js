var Lock = function () {
    return {
        //main function to initiate the module
        init: function () {
             $.backstretch([
		        "img/bg/1.jpg",
		        "img/bg/2.jpg",
		        "img/bg/3.jpg",
		        "img/bg/4.jpg"
		        ], {
		          fade: 1000,
		          duration: 8000
		      });
        }
    };
}();