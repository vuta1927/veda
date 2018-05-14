import { Component, OnInit, OnDestroy, ViewEncapsulation, ViewChild, ViewChildren, QueryList, ViewContainerRef } from "@angular/core";
import { ActivatedRoute, Router, RouterStateSnapshot } from "@angular/router";
import CustomStore from 'devextreme/data/custom_store';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../helpers';
import { ToastsManager } from 'ng2-toastr/ng2-toastr';
import { SecurityService } from '../../shared/services/security.service';
import { Constants } from '../../constants';
import { ClassService } from '../services/class.service';
import { ProjectUserService } from '../services/project-users.service';
import {MergeClass} from './merge-class/merge-class.component';
import { DataService } from '../data.service';
import DataSource from 'devextreme/data/data_source';
import { Merge, QcOption, FilterOptions } from '../../shared/models/merge.model';
@Component({
    selector: 'app-merge-project',
    styleUrls: ['merge-project.component.css'],
    templateUrl: 'merge-project.component.html',
    encapsulation: ViewEncapsulation.None
})

export class MergeProject implements OnInit {
    @ViewChild(DxDataGridComponent) dataGrid: DxDataGridComponent;
    selectedUsers: any[] = [];
    selectedClasses: any[] = [];
    classSource: any = {};
    userSource: any = {};
    newUserSource: any = {};
    orginClassDataSource: any;
    orginUserDataSource: any;
    projectName: string = '';
    levelTotal: number = 5;
    qcFilterOptions: QcOption[] = [];
    constructor(
        private route: Router,
        private activatedRoute: ActivatedRoute,
        public toastr: ToastsManager,
        private vcr: ViewContainerRef,
        private classService: ClassService,
        private modalService: NgbModal,
        private dataService: DataService,
        private projectUserSerivce: ProjectUserService
    ) {
        this.toastr.setRootViewContainerRef(vcr);
        let mother = this;
        activatedRoute.params.subscribe(params => {
            mother.classService.getClassProjects(params.projects)
            .toPromise().then(response => {
                mother.dataService.changeClass(response.result);
                mother.classSource = response.result;
                mother.orginClassDataSource = Object.assign([], response.result);
            });
            mother.projectUserSerivce.GetProjectUsersForMerge(params.projects)
            .toPromise().then(response => {
                mother.userSource = response.result;
                mother.orginUserDataSource = Object.assign([], response.result);
            });
        });
    }

    ngOnInit(): void {

    }

    ngOnDestroy(): void {
        //Called once, before the instance is destroyed.
        //Add 'implements OnDestroy' to the class.
    }

    cancel(): void {
        this.route.navigate(['project']);
    }

    SaveAll(): void {
        let mergeData: Merge = new Merge();
        let  filterOptions: FilterOptions = new FilterOptions();
        filterOptions.qcOptions = this.qcFilterOptions;

        mergeData.projectName = this.projectName;
        mergeData.classes = this.classSource;
        mergeData.filterOptions = filterOptions;
        console.log(mergeData);
    }

    qcLevelChange(e){
        var ele = e.target;
        var qcOpt = this.qcFilterOptions.find(x=>x.index == ele.name);
        if(qcOpt){
            qcOpt.value = ele.value;
        }else{
            this.qcFilterOptions.push({ index: ele.name ,value: ele.value});
        }
    }

    openMergeClassModal(): void {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        const modalRef = this.modalService.open(MergeClass, config);
        if (this.classSource)
            modalRef.componentInstance.classes = this.classSource;
        
        if(this.selectedClasses)
            modalRef.componentInstance.mergeClasses = this.selectedClasses;

        modalRef.componentInstance.projectName = this.projectName? this.projectName : 'New Project';

        var mother = this;
        modalRef.result.then(function () {
            mother.dataService.currentClass.subscribe(p => {
                mother.classSource = p;
                console.log(p);
            });
            // mother.dataGrid["first"].instance.refresh();
            // mother.classSource.reload();
        })
    }

    classSelectionChanged(data: any): void {
        var selectedKeys = this.dataGrid.instance.getSelectedRowKeys();
        if(data.selectedRowsData.length > 2){
            for (var i = 0; i < selectedKeys.length; i++) {
                if (selectedKeys[i].project == data.currentSelectedRowKeys[0].project && selectedKeys[i].name == data.currentSelectedRowKeys[0].name) {
                    this.dataGrid.instance.deselectRows([selectedKeys[i]]);
                }
            }
        }
        this.selectedClasses = data.selectedRowsData;
    }

    projectNameChange(): void {
        var mother = this;
        this.classSource.forEach(e => {
            if(e.id == 0){
                e.project = mother.projectName;
            }
        });
    }

    resetClass(): void {
        this.classSource =  Object.assign([], this.orginClassDataSource);
    }

    removeClass(): void {
        if(!this.selectedClasses.length){
            return;
        }

        this.selectedClasses.forEach(klass => {
            var e = this.classSource.find(x=>x.name == klass.name);
            if(e){
                this.classSource.splice(this.classSource.indexOf(e), 1);
            }
        });
    }
}