const myButton = document.getElementsByClassName('btn1');


for (const btn of myButton) {
    btn.addEventListener('click', function(e) {
        e.preventDefault(); //This prevents the default action
        //alert("This is scary"); //Show the alert  
        //myText.innerHTML="Tomer";
        
        const btnID = btn.id;
        const p = document.getElementById("text "+btnID)
        
        if (p.className == "txt1")
            p.className = "txt2";
        else p.className = "txt1";

    });

}



/*
for (const btn of myButton) {
    btn.addEventListener('click', function(e) {
        e.preventDefault(); //This prevents the default action
        //alert("This is scary"); //Show the alert  
        //myText.innerHTML="Tomer";

        const temp = myText[1].className;
        if (temp == "txt1")
            myText[1].className = "txt2";
        else myText[1].className = "txt1";
        
    });
    
}

for(let i=0;i<myButton.length;i++){
    myButton[i].addEventListener('click', function(e) {
        e.preventDefault(); //This prevents the default action
        

        const id = myButton[i].id;
        const txt = document.getElementById(id);
        if(txt.className == "txt1")
            txt.className = "txt2";
        else txt.className = "txt1";

    });
    
}*/

