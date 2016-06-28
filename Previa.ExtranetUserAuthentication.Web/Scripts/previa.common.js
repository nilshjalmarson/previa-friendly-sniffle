// Declare the Previa-object
var Previa = {};
// Declare the Common-object
Previa.Common = {};

$(document).ready(function () {
   Previa.Common.Initialize();
});

Previa.Common.Initialize = function () {
   var spinneropts = {
      lines: 13, // The number of lines to draw
      length: 3, // The length of each line
      width: 2, // The line thickness
      radius: 4, // The radius of the inner circle
      color: '#000', // #rgb or #rrggbb
      speed: 1, // Rounds per second
      trail: 31, // Afterglow percentage
      shadow: false, // Whether to render a shadow
      hwaccel: false // Whether to use hardware acceleration
   };
   var spinnertarget = document.getElementById("loader");
   var spinner = new Spinner(spinneropts).spin(spinnertarget);

   // hook up the loader
   $("button[type=submit]").on("click", function () {
      if ($("form").valid())
         $("#loader").show();
   });

   // ie focus hack
   if ($.browser.msie) {
      $("input").on("focus", function () {
         $(this).addClass("iefocus");
      }).on("blur", function () {
         $(this).removeClass("iefocus");
      });

      $("input#Username").addClass("iefocus").select();
   }

   // select first element on page
   $(":input:visible:enabled:first").focus().select();
};
