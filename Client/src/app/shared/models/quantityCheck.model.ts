export interface QuantityCheck{
    id: number;
    qCDate: Date;
    value1: boolean;
    value2: boolean;
    value3: boolean;
    value4: boolean;
    value5: boolean;
    comment: string;
}

export class QuantityCheckForView{
    level: number;
    value: boolean;
    comment: string;
    href: string;
    collapseId: string;
}