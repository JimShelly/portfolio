const createError = require('http-errors');
const express = require('express');
const path = require('path');
const cookieParser = require('cookie-parser');
const logger = require('morgan');
const graphqlHttp = require('express-graphql');
const { buildSchema } = require('graphql');

//const graphqlSchema = require('./graphql/schema');
//const graphqlController = require('./graphql/resolvers');

const {
  GraphQLSchema,
  GraphQLObjectType,
  GraphQLList,

} = require('graphql');

const PerfumeType = new GraphQLObjectType({
  name: 'perfume'
})

const RootQueryType = new GraphQLObjectType({
  name: 'Query',
  description: 'Root Query',
  fields: () => ({
    perfumes: {
      type: new GraphQLList(PerfumeType),
      descript: 'List of Perfumes',
      resolve: () => perfumes
    }
  })
})
var indexRouter = require('./routes/index');
var usersRouter = require('./routes/users');

var app = express();

// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');

app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));
app.use(
  '/graphql', 
  graphqlHttp({
  schema: buildSchema(`
    type RootQuery {
      perfumes: [String!]!
    }

    type RootMutation {
      createPerfume(name: String): String
    }
    schema {
      query: RootQuery
      mutation: RootMutation
    }
  `),
  rootValue: {
    perfumes: () => {
      return ['Chanel', 'Liz Tayler', 'Summer Breeze']
    },
    createPerfume: (args) => {
      const perfumeName = args.name;
      return perfumeName;
    }
  },
  graphiql: true,
}));

app.use('/', indexRouter);
app.use('/users', usersRouter);

// catch 404 and forward to error handler
app.use(function(req, res, next) {
  next(createError(404));
});

// error handler
app.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};

  // render the error page
  res.status(err.status || 500);
  res.render('error');
});

module.exports = app;
