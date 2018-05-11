export interface Iimage{
    id: string;
    path: string;
    ignored: boolean;  
    totalClass: number;
    tagHasClass: number;  
    tagNotHasClass: number;  
    taggedDate: Date;
    classes: string;
    qcStatus: string;
    userQc: string;  
    qcDate: Date;
    userTagged: string;
    tagTime: number;
    userUsing: string;
}