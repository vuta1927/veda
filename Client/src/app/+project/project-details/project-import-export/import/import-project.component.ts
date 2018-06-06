import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ImportExportService } from '../../../services/import-export.service';
import { DataService } from '../../data.service';
import swal from 'sweetalert2';
import { ConsoleLogger } from '@aspnet/signalr';
import { Helpers } from '../../../../helpers';
import { ProjectService } from '../../../project.service';
import { ClassService } from '../../../services/class.service';
import responsive_box from 'devextreme/ui/responsive_box';
import { ClassMap } from '../../../../shared/models/project.model';
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
    classes = [];
    classMappingTable:ClassMap[] = [];
    constructor(
        private importService: ImportExportService,
        private dataSerivce: DataService,
        private route: ActivatedRoute,
        private router: Router,
        private projectService: ProjectService,
        private classSerivce: ClassService
    ) {

    }

    ngOnInit() {
        Helpers.setLoading(true);
        this.route.queryParams.subscribe(params => {
            if(params.project){
                this.projectService.getProjectById(params.project).toPromise().then(Response=>{
                    if(Response.result){
                        this.project = Response.result;
                        this.classSerivce.getClasses(this.project.id).toPromise().then(resp=>{
                            Helpers.setLoading(false);
                            if(resp.result){
                                this.classes = resp.result;
                                this.classes.forEach(c => {
                                    this.classMappingTable.push(new ClassMap(c.id, -1));
                                });
                            }
                        }).catch(Response=>{
                            Helpers.setLoading(false);
                            if(Response.status == 401 || Response.status == 403){
                                this.router.navigate(['#']);
                                return;
                            };
                            swal({text:"cant get class!", type:"error"});
                        })
                    }
                }).catch(Response=>{
                    Helpers.setLoading(false);
                    if(Response.status == 401 || Response.status == 403){
                        this.router.navigate(['#']);
                        return;
                    };
                    swal({text:Response.error? Response.error.text:Response.message, type:"error"});
                })
            }
        })

        console.log(this.project);
    }

    idBoxChange(classId, event){
        var value = event.target.value;
        var cm = this.classMappingTable.find(x=>x.classId == classId);
        if(cm){
            cm.value = value;
        }
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
        if(this.classMappingTable.find(x=>x.value < 0)){
            swal({text:"Missing ID! Please fill all ID box!", type:"error"}).then(()=>{return});
        }
        this.uploading = true;
        if (!this.totalFile)
            this.totalFile = this.uploadfiles.length;
        // let totalFile = this.uploadfiles.length;

        Helpers.setLoading(true);
        if (this.uploadfiles.length > 0 && this.uploadedFile <= (this.totalFile - 1)) {
            let file = this.uploadfiles[this.uploadedFile];
            const formData = new FormData();
            formData.append(file.name, file);
            formData.append('data',JSON.stringify(this.classMappingTable));
            this.importService.Import(this.project.id, formData).toPromise().then(Response => {
                Helpers.setLoading(false);
                swal({
                    title: '', text: 'import success', type: 'success'
                }).then(() => {
                    window.location.reload();
                });
            }).catch(resp => {
                Helpers.setLoading(false);
                if(resp.status == 401 || resp.status == 403){
                    this.router.navigate(['#']);
                    return;
                };
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
            }).catch(resp => {
                if(resp.status == 401 || resp.status == 403){
                    this.router.navigate(['#']);
                    return;
                };
                swal({
                title: '', text: resp.error.message, type: 'error'
            });
        })

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