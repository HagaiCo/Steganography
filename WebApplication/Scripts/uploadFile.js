

document.querySelectorAll(".form-control-uploadfile").forEach(inputElement => {
    const dropZoneElement = inputElement.closest(".drop-zone");
    
    
    dropZoneElement.addEventListener("click", e => {
        inputElement.click();
    });
    
    inputElement.addEventListener("change", e => {
        if (inputElement.files.length) {
            updateThumbnail(dropZoneElement, inputElement.files[0]);
            // dropZoneElement.classList.add("drop-zone--over");
        }
    });
    
    dropZoneElement.addEventListener("dragover", e => {
        e.preventDefault();
        dropZoneElement.classList.add("drop-zone--over");
    });
    
    ["dragleave", "dragend"].forEach(type => {
        dropZoneElement.addEventListener(type, e => {
            dropZoneElement.classList.remove('drop-zone--over');
        });
    });
    
    dropZoneElement.addEventListener("drop", e => {
        e.preventDefault();
        
        if (e.dataTransfer.files.length){
            inputElement.files = e.dataTransfer.files;
            updateThumbnail(dropZoneElement, e.dataTransfer.files[0]);
        }
        
        dropZoneElement.classList.remove("drop-zone--over");
    });
});


// @param {HTMLElement} dropZoneElement
// @param {File} file

function updateThumbnail(dropZoneElement, file) {
    let thumbnailElement = dropZoneElement.querySelector(".drop-zone__thumb");
    
    console.log(file);
    
    
    if (dropZoneElement.querySelector(".control-label-drop")) {
        dropZoneElement.querySelector(".control-label-drop").remove();
    }
    
    // first time there is now thumbnailElement, so lets create it
    if (!thumbnailElement){
        thumbnailElement = document.createElement("div");
        thumbnailElement.classList.add("drop-zone__thumb");
        dropZoneElement.appendChild(thumbnailElement);
    }
    
    thumbnailElement.dataset.label = file.name;
    
    //show thumbnail for image file
    if (file.type.startsWith("image/")) {
        const reader = new FileReader();
        
        reader.readAsDataURL(file);
        reader.onload = () => {
          thumbnailElement.style.backgroundImage = `url('${reader.result}')`;  
        };
    }else {
        thumbnailElement.style.backgroundImage = null;
    }
}
