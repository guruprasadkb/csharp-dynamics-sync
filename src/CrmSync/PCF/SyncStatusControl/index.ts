/**
 * Power Apps Component Framework (PCF) - Sync Status Control
 * 
 * Contoso uses this control on the Account and Contact forms to visualize
 * the sync status from the external LegacyPro system.
 */

// NOTE TO CANDIDATE: 
// You don't need to build this using npm. 
// Just implement the 'getSyncStatusMessage' logic and the 'updateView' method.

export class SyncStatusControl {
    private _container!: HTMLDivElement;
    private _context: any; // pcf.Context
    private _notifyOutputChanged!: () => void;

    public init(context: any, notifyOutputChanged: () => void, container: HTMLDivElement): void {
        this._context = context;
        this._notifyOutputChanged = notifyOutputChanged;
        this._container = container;
        this.updateView(context);
    }

    public updateView(context: any): void {
        this._context = context;
        const lastSyncedOn = context.parameters.lastSyncedOn.raw;
        
        // TODO: Implement getSyncStatusMessage logic
        const statusMessage = this.getSyncStatusMessage(lastSyncedOn);
        
        // Render the message in the container
        this._container.innerHTML = `
            <div style="padding: 10px; border: 1px solid #ddd; border-radius: 4px;">
                <strong>Sync Status:</strong> 
                <span id="sync-status" class="${this.getStatusClass(statusMessage)}">
                    ${statusMessage}
                </span>
            </div>
        `;
    }

    /**
     * Determines the status message based on the last sync date.
     * @param lastSynced The date/time the record was last synced.
     * @returns string Status message
     */
    public getSyncStatusMessage(lastSynced: Date | null): string {
        // TODO: IMPLEMENT ME
        // Rules:
        // 1. If null -> "Not Synced"
        // 2. If synced today -> "Synced recently"
        // 3. Otherwise -> "Sync Pending Update"
        return "TODO";
    }

    private getStatusClass(message: string): string {
        switch(message) {
            case "Synced recently": return "status-success";
            case "Sync Pending Update": return "status-warning";
            case "Not Synced": return "status-error";
            default: return "";
        }
    }
}
