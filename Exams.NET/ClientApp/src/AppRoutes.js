import ApiAuthorzationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import TestAdmin from "./components/TestAdmin";

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
  ...ApiAuthorzationRoutes
];

export default AppRoutes;
