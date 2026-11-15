export type TestDefinition = {
    config: TestConfig;
    input: string;
    html: string;
    plain: string;
    nodes: TestNode;
};

export type TestConfig = {
    base?: string;
    elements?: string[];
    tags?: string[];
    processors?: string[];
};

export type TestNode = {
    name?: string;
    type: string;
    pos: [number, number];
    children?: TestNode[];
    text?: string;
    html?: [string, string];
    plain?: [string, string];
    key?: string;
    value?: JsonNode;
};

export type JsonNode = string | Record<string, unknown> | null;
