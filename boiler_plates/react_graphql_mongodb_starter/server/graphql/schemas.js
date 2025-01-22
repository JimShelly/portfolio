const { buildSchema } = require('graphql');
const perfumeSchemas = require('./schemas/perfumes');
const userSchemas = require('./schemas/user');

module.exports = buildSchema(`
    ${userSchemas.User}
    ${perfumeSchemas.Perfume}
    ${userSchemas.AuthData}
    ${userSchemas.UserInputData}
    ${perfumeSchemas.PerfumeInputData}

    type RootQuery {
        ${userSchemas.UserQueries}
        ${perfumeSchemas.PerfumeQueryies}
    }

    type RootMutation {
        ${userSchemas.UserMutation}
        ${perfumeSchemas.PerfumeMutation}
    }

    schema {
        query: RootQuery
        mutation: RootMutation
    }
`)