import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-export-project',
    templateUrl : 'export-project.component.html',
    styleUrls: ['export-project.component.css']
})

export class ExportProjectComponent implements OnInit {
    uploadfiles: any = [];
    uploading = false;
    totalFile = 0;
    uploadedFile = 0;
    allowedExtensions = ['rar', 'zip', 'RAR', 'ZIP', '7z', '7Z'];
    messageHeader: string;
    message: string;
    
    constructor(){

    }

    ngOnInit(){

    }

    appendUploadFiles(files) {
        this.message = '';
        for(var i=0; i < files.length; i++){
            var filename = files[i].name;
            var fileExtension = filename.split('.')[1];
            console.log(fileExtension);
            if(this.allowedExtensions.indexOf(fileExtension) == -1){
                this.message = 'File extension not allowed!';
                return;
            }        
        }
        this.uploadfiles = files;
    }

    upload() {
        // if (this.uploadfiles.length === 0)
        //     return;

        this.uploading = true;
        if (!this.totalFile)
            this.totalFile = this.uploadfiles.length;
        // let totalFile = this.uploadfiles.length;

        if (this.uploadfiles.length > 0 && this.uploadedFile <= (this.totalFile - 1)) {
            let file = this.uploadfiles[this.uploadedFile];
            const formData = new FormData();
            formData.append(file.name, file);
            

        } else {
            this.uploadfiles = [];
            this.totalFile = 0;
            this.uploadedFile = 0;
        }

    }

    uploadFile(file) {
        if (file.size == 0)
            return;

        const formData = new FormData();

        formData.append(file.name, file);
    }

    removeFile(file) {
        let tempFiles = [];
        for (let f of this.uploadfiles) {
            if (f.name == file.name) continue;
            tempFiles.push(f);
        }
        this.uploadfiles = [];
        this.uploadfiles = tempFiles;
    }

    clear() {
        this.uploadfiles = [];
        $('#successMessage').css("display", "none");
        $('#errorMessage').css("display", "none");
    }
}