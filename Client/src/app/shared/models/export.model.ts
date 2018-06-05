export class QcOption {
    constructor(
        public index: number,
        public value: boolean) { }
}

export class ExportClass {
    constructor(
        public projectId: string,
        public classes: string[],
        public qcOptions: QcOption[]) {

    }
}