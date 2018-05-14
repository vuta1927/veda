import { Class } from './class.model';

export class Merge {
    projectName: string = '';
    classes: Class[] = [];
    mergeClasses: MergeClass[] = [];
    projects: string[] = [];
    users: MergeProjectUser[] = [];
    filterOptions: FilterOptions = new FilterOptions();
}

export class MergeProjectUser{
    userName: string = '';
    roleName: string = '';
}

export class MergeClass{
    classes: Class[] = [];
    newClassName: string = '';
}

export class FilterOptions{
    qcOptions: QcOption[] = [];
}

export class QcOption{
    index: number;
    value: number;
}