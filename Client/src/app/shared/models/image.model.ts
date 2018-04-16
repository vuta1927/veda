export interface Iimage{
    id: string;
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
}