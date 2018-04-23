export class MessageTypes {
    constructor(
        public UserUsing = "userUsing",
        public UserRelease = "userRelease",
        public ReloadImageData = "reloadImageData",
        public Send = "Send"
    ) { }

    public getAll(): string[] {
        return [this.UserUsing, this.UserRelease, this.ReloadImageData, this.Send];
    }
}