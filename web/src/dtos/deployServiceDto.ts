

export default interface DeployServiceDto {
    id: number;
    groupName?: string | null;
    name: string;
    deployMode: number;
    projectPath: string;
    isSelfContained: boolean;
    enableHealthCheck: boolean;
    enableNotify?: boolean | null;
    description?: string | null;
    environmentName?: string | null;
    port?: number | null|undefined;
}
