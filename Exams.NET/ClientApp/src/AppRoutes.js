import ApiAuthorzationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import TestAdmin from "./components/TestAdmin";
import QuestionForm from "./components/QuestionForm";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/fetch-data',
    requireAuth: true,
    element: <FetchData />
  },
  {
    path: 'testAdmin',
    requireAuth: true,
    element: <TestAdmin />
  },
  {
    path: '/questions',
    requireAuth: true,
    element: <QuestionForm />
  },
  ...ApiAuthorzationRoutes
];

export default AppRoutes;
