exports.Perfume = `
type Perfume {
    _id: ID!
    name: String!
    description: String!
    point: Int!
    price: Float!
    date: String!
    owner: [User!]!
}`;

exports.PerfumeInputData = `
input PerfumeInputData {
    name: String!
    description: String!
    price: Float!
    date: String!
}`;

exports.PerfumeQueries = `
    perfumes(): [Perfume!]!
`;

exports.PerfumeMutations = `
    createPerfume(perfumeInput: PerfumeInputData!): Perfume!
    updatePerfume(perfumeId: ID!, perfumeInput: PerfumeInputData!): Perfume!
    assignPerfume(perfumeId: ID!): Perfume!
    deletePerfume(perfumeId: ID!): Perfume!
`;
