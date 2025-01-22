const userResolver = require('./resolvers/user');
const perfumResolver = require('./resolvers/perfume');

module.exports = {
    ...userResolver,
    ...perfumResolver
};