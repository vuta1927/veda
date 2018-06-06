import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ClassService } from '../../../services/class.service';
import { DataService } from '../../data.service';
import { QcOption } from '../../../../shared/models/merge.model';
import { ExportClass } from '../../../../shared/models/export.model';
import { ProjectService } from '../../../project.service';
import { ProjectSettingService } from '../../../../+administrator/settings/settings.service';
import swal from 'sweetalert2';
@Component({
    selector: 'app-export-project',
    templateUrl : 'export-project.component.html',
    styleUrls: ['export-project.component.css']
})

export class ExportProjectComponent implements OnInit {
    messageHeader: string;
    message: string;
    currentProject: any = {};
    classSource: any = {};
    selectedClasses: any[] = [];
    qcOptions: QcOption[] = [];
    qcLeves:any;
    constructor(
        private classService: ClassService,
        private dataService: DataService,
        private projectService: ProjectService,
        private projectSettingService: ProjectSettingService,
        private router: Router
    ){
        this.qcLeves = Array(5).fill(1).map((x, i) => i + 1);
        const mother = this;
        this.dataService.currentProject.subscribe(p=>{
            mother.currentProject = p;
            mother.classService.getClasses(p.id).toPromise().then(response => {
                mother.classSource = response.result;
                mother.projectSettingService.getSetting(p.id).toPromise().then(Response=>{
                    if(Response.result){
                        mother.qcLeves = Array(Response.result.quantityCheckLevel).fill(1).map((x, i) => i + 1)
                        console.log(mother.qcLeves);
                    }
                }).catch(err=>{
                    if(err.status == 401 || err.status == 403){
                        mother.router.navigate(['#']);
                        return;
                    };
                    if(err.error){
                        console.log(err.error.text);
                    }else{
                        console.log(err.message);
                    }
                });
            });
        });
        
    }

    ngOnInit(){

    }

    classSelectionChanged(data:any){
        this.selectedClasses = data.selectedRowsData;
    }

    classSelected(data){

    }

    qcLevelChange(e): void {
        const ele = e.target;
        const qcOpt = this.qcOptions.find(x => x.index === ele.name);
        if (qcOpt) {
            if(ele.value == 3){
                this.qcOptions.splice(this.qcOptions.indexOf(qcOpt),1);
            }else{
                qcOpt.value = ele.value;
            }
        } else {
            if(ele.value == 3) return;
            this.qcOptions.push(new QcOption(ele.name, ele.value == 1));
        }
    }

    Export(){
        const mother = this;
        var classes = [];
        this.selectedClasses.forEach(c=>{
            classes.push(c.name);
         });
         
        const exportData = new ExportClass(this.currentProject.id, classes, this.qcOptions);
        this.projectService.Export(exportData).toPromise().then(Response=>{
            swal({text:'Export is processing. An download link will be send to your email when export complete!', type:"success"})
            // var blob = new Blob([Response], {type: "application/zip"});
            // mother.saveAs(blob, mother.currentProject.name+".zip");
        }).catch(resp=>{
            if(resp.status == 401 || resp.status == 403){
                mother.router.navigate(['#']);
                return;
            };
            swal({
                title: '',text: resp.error.message, type: 'error'
            })
        });
    }

    saveAs(blob, fileName) {
        var url = window.URL.createObjectURL(blob);
        var doc = document.createElement("a");
        doc.href = url;
        doc.download = fileName;
        doc.click();
        window.URL.revokeObjectURL(url);
    }
}