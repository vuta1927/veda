import { Component, OnInit } from '@angular/core';
import { ImportExportService } from '../../../services/import-export.service';
import { DataService } from '../../data.service';
import swal from 'sweetalert2';
import { ConsoleLogger } from '@aspnet/signalr';
import { Helpers } from '../../../../helpers';
@Component({
    selector: 'app-import-project',
    templateUrl: 'import-project.component.html',
    styleUrls: ['import-project.component.css']
})

export class ImportProjectComponent implements OnInit {
    uploadfiles: any = [];
    uploading = false;
    totalFile = 0;
    uploadedFile = 0;
    allowedExtensions = ['rar', 'zip', 'RAR', 'ZIP', '7z', '7Z'];
    messageHeader: string;
    message: string;
    project: any = {};
    constructor(
        private importService: ImportExportService,
        private dataSerivce: DataService
    ) {

    }

    ngOnInit() {
        this.dataSerivce.currentProject.subscribe(p => this.project = p);
        console.log(this.project);
    }

    appendUploadFiles(files) {
        this.message = '';
        for (var i = 0; i < files.length; i++) {
            var filename = files[i].name;
            var fileExtension = filename.substr(filename.length - 3);
            if (this.allowedExtensions.indexOf(fileExtension) == -1) {
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
            Helpers.setLoading(true);
            this.importService.Import(this.project.id, formData).toPromise().then(Response => {
                Helpers.setLoading(false);
                swal({
                    title: '', text: 'import success', type: 'success'
                }).then(() => {
                    window.location.reload();
                });
            }).catch(resp => {
                Helpers.setLoading(false);
                swal({
                    title: '', text: resp.error.text, type: 'error'
                });
            })

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

        if (this.uploadfiles.length > 0 && this.uploadedFile <= (this.totalFile - 1)) {
            let file = this.uploadfiles[this.uploadedFile];
            const formData = new FormData();
            formData.append(file.name, file);

            this.importService.Import(this.project.id, formData).toPromise().then(Response => {
                swal({
                    title: '', text: 'import success', type: 'success'
                });
            }).catch(resp => swal({
                title: '', text: resp.error.message, type: 'error'
            }))

        } else {
            this.uploadfiles = [];
            this.totalFile = 0;
            this.uploadedFile = 0;
        }
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