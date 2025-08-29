This README will briefly introduce where and how to modify the files

1. The data flow of one webpage (take player.php for example)
    The player.php file is the DISPLAY page, this means that everything, including queries, will be showing on this page. The language used here is mainly HTML, but two JavaScript files (jQuery & player.js) will be attached at the bottom of the page.

    JQuery here is used for loading the php file dynamically without refreshing the page. It can also help access the html elements more conveniently. player.js is where we call the query function, player_query.php, and also can do some animations and computations.

    Inside player_query.php is where we connect to the database and request queries. It will also return the html data back to player.php for displaying.

2. How to call player_query.php
    First, we need to know how jQuery select an element (it's '$'). After selecting an element, we can add function to it. For instance, "$(...).on(...)" can make the element detect an event (clicking, loading, etc.), or "$(...).remove()" to delete the element. You can check the code in player.js to see some examples.
    JQuery has a function called "load()", which will load the file to a selected element. For example,

                                        $("#example").load("input.txt")
    
    load "input.txt" into an element with id = "example". We can do the same to a php file and the method is shown below.

3. How to load php into element
    When loading the php file in jQuery, it doesn't actually inject the entire file into the element. Instead, it returns the "output". To make php output stuff, we use "echo". Inside echo, we can put HTML-formatted string so that when the file returns, player.php can display additional structures like tables or search options.

4. The location to load php
    Inside each display page, there should have an element <tbody> with id="table-content". You can use $("#table-content") to access it. Additionally, you can find a <div> with id="test" in player.php and country.php. Here, we display the profile of athletes and countries.