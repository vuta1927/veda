import { Component, ViewEncapsulation, ViewChild, ViewChildren, QueryList } from '@angular/core';
import { RolesService } from '../roles/roles.service';
import CustomStore from 'devextreme/data/custom_store';

import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

import { DxDataGridComponent } from 'devextreme-angular';
import { Helpers } from '../../helpers';
import { CreateOrUpdateRoleComponent } from './create-or-update/create-or-update-role.component';
import { Role } from '../../shared/models/role.model';
import { PermissionService } from './permission/permission.service';
import { PermissionCategory } from '../../shared/models/permission.model';
@Component({
    selector: 'app-roles',
    templateUrl: './roles.component.html',
    styleUrls: ['./roles.component.scss'],
    encapsulation: ViewEncapsulation.None
})
export class RolesComponent {
    @ViewChildren(DxDataGridComponent) dataGrids: QueryList<DxDataGridComponent>
    dataSource: any = {};
    permissionSource: any;
    selectedRole: any;
    permissions: PermissionCategory[];
    constructor(private roleService: RolesService, private modalService: NgbModal, private permissionService: PermissionService) {
        this.dataSource.store = new CustomStore({
            load: function (loadOptions: any) {
                return roleService.getRawRoles()
                    .toPromise()
                    .then(response => {
                        return {
                            data: response.result,
                            totalCount: response.result.length
                        }
                    })
                    .catch(error => { throw 'Data Loading Error' });
            }
        });
    }

    refreshAllGrids() {
        this.dataGrids.forEach(function (dataGrid) {
            // console.log(dataGrid.instance.element().id);
            let gridId = dataGrid.instance.element().id
            if (gridId == "gridRolesContainer")
                dataGrid.instance.refresh();
        })
    }

    refreshPermissionGrids() {
        this.dataGrids.forEach(function (dataGrid) {
            // console.log(dataGrid.instance.element().id);
            let gridId = dataGrid.instance.element().id
            if (gridId == "gridPermissionsContainer")
                dataGrid.instance.refresh();
        });
        Helpers.setLoading(false);
    }

    onCheckboxChecked(e, data){
        Helpers.setLoading(true);
        // console.log(e, data);
        this.permissions.forEach(permission => {
            if(permission.id == data.data.id){
                permission.isCheck = data.data.isCheck;
                this.permissionService.UpdatePermission(permission, this.selectedRole.id).toPromise().then(Response=>{
                    Helpers.setLoading(false);
                    this.refreshPermissionGrids();
                });
            }
        });
        // console.log(this.permissions);
    }
    onSelectionChanged(e) { // Handler of the "selectionChanged" event
    this.selectedRole = e.currentSelectedRowKeys[0];
        if (this.selectedRole){
            Helpers.setLoading(true);
            this.permissionService.getAllPermissionCategory(this.selectedRole.id).toPromise().then(Response=>{
                if(Response.result){
                    Helpers.setLoading(false);
                    this.permissions = new Array<PermissionCategory>();
                    this.permissions = <PermissionCategory[]>Response.result;
                    this.permissionSource = this.permissions;
                }
                
                Helpers.setLoading(false);
            });
        }
            
    }

    addNewRole() {
        this.openCreateOrUpdateModal();
    }
    addPermissionToRole(){

        // if(this.selectedRole)
        //     this.openAddPermissionModal(this.selectedRole);
        // else{
        //     alert("please select a role to add permission on!");
        // }
    }

    roleEditClick($event, data) {
        // console.log("edit", data.data.roleName);
        this.openCreateOrUpdateModal(data.data);
    }

    roleDeleteClick($event, data) {
        this.roleService.Deleterole(data.data.id).toPromise().then(Response => {
            this.refreshAllGrids();
            Helpers.setLoading(false);
        });
        // console.log("delete", data.data.roleName);
    }

    openCreateOrUpdateModal(role?: Role) {
        const config = {
            keyboard: false,
            beforeDismiss: () => false
        }
        const modalRef = this.modalService.open(CreateOrUpdateRoleComponent, config);
        modalRef.componentInstance.role = role;
        var mother = this;
        modalRef.result.then(function () {
            mother.refreshAllGrids();
        })
    }
}