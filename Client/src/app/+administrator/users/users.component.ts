import { Component, ViewEncapsulation, ViewChild } from '@angular/core';
import { UsersService } from './users.service';
import CustomStore from 'devextreme/data/custom_store';

import 'rxjs/add/operator/toPromise';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { DxDataGridComponent } from 'devextreme-angular';
import { CreateOrUpdateUserComponent } from './create-or-update-user.component';
import { TranslateExtService } from '../../shared/services/translate-ext.service';
import { Helpers } from '../../helpers';
import swal from 'sweetalert2';
@Component({
    selector: 'app-users',
    templateUrl: './users.component.html',
    styleUrls: [
        './users.component.scss'
    ],
    encapsulation: ViewEncapsulation.None
})
export class UsersComponent {
    @ViewChild(DxDataGridComponent) dataGrid: DxDataGridComponent;
    menuItems = [
        {
            icon: 'preferences',
            items: [
                { text: this.translate.get('Edit'), icon: 'la la-edit', value: 1 },
                { text: this.translate.get('Delete'), icon: 'la la-trash', value: 2 }
            ]
        }
    ]

    dataSource: any = {};
    selectedUsers: any = [];
    constructor(private usersService: UsersService, private modalService: NgbModal, private translate: TranslateExtService) {

        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                var params = '';

                params += loadOptions.skip || 0;
                params += '/' + loadOptions.take || 12;

                // if (loadOptions.sort) {
                //     params += '&orderBy=' + loadOptions.sort[0].selector;
                //     if (loadOptions.sort[0].desc) {
                //         params += ' desc';
                //     }
                // }

                return usersService.getUsers(params)
                    .toPromise()
                    .then(response => {
                        return {
                            data: response.result,
                            totalCount: response.result.length
                        };
                    })
                    .catch(error => { });
            }
        });
    }

    exportToExcel() {
        this.dataGrid.instance.exportToExcel(false);
    }

    selectionChanged(data:any){
        this.selectedUsers = data.selectedRowsData;
    }

    openCreateOrUpdateModal(id?: number) {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        Helpers.setLoading(true);
        let mother = this;
        this.usersService.getUserForCreatOrEdit(id).subscribe(res => {
            const modalRef = this.modalService.open(CreateOrUpdateUserComponent, config);
            modalRef.componentInstance.userForCreateOrEdit = res.result;
            modalRef.result.then(function () {
                mother.refreshGrids();
            })
            Helpers.setLoading(false);
        }, _ => {
            Helpers.setLoading(false);
        });
    }
    
    refreshGrids() {
        this.dataGrid.instance.refresh();
    }

    itemClick($event, data) {
        this.openCreateOrUpdateModal(data.data.id);
    }
    
    deleteSelectedUser(){
        var userids = '';
        for(var i=0; i < this.selectedUsers.length; i++){
            if(i == this.selectedUsers.length - 1){
                userids += this.selectedUsers[i].id;
            }else{
                userids += this.selectedUsers[i].id + ";";
            }
        };
        
        Helpers.setLoading(true);
        this.usersService.deleteUse(userids).toPromise().then(Response=>{
            Helpers.setLoading(false);
            swal({text:"Users removed!", type:"success"}).then(()=>{
                this.refreshGrids();
            });
        }).catch(Response=>{
            Helpers.setLoading(false);
            swal({text:Response.error? Response.error.text: Response.message, type:"error"});
        });
    }
}