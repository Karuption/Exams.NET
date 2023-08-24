import ApiAuthorzationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import {Home} from "./components/Home";
import TestAdmin from "./components/TestAdmin";
import UserTest from "./components/UserTest";
import {QuestionAdministration} from "./components/QuestionAdministration";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/testAdmin',
    requireAuth: true,
    element: <TestAdmin />
  },
  {
    path: '/questionAdmin',
    requireAuth: true,
    element: <QuestionAdministration />
  },
  {
    path: '/Test',
    requireAuth: true,
    element: <UserTest testId={1} />
  },
  ...ApiAuthorzationRoutes
];

export default AppRoutes;
