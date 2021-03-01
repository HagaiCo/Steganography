const myButton = document.getElementsByClassName('btn1');
const myText = document.getElementById('txt1');


for (const btn of myButton) {
    btn.addEventListener('click', function(e) {
        //e.preventDefault(); //This prevents the default action
        //alert("This is scary"); //Show the alert  
        //myText.innerHTML="Tomer";

        const temp = myText.className;
        if (temp == "txt1")
            myText.className = "txt2";
        else myText.className = "txt1";
    });


}