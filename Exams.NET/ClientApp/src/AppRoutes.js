import ApiAuthorzationRoutes from "./components/api-authorization/ApiAuthorizationRoutes";
import { Home } from "./components/Home";
import TestAdmin from "./components/TestAdmin";
import UserTest from "./components/UserTest";
import { QuestionAdministration } from "./components/QuestionAdministration";
import { UserPortal } from "./components/UserPortal";

const AppRoutes = [
   {
      index: true,
      element: <Home />,
   },
   {
      path: "/testAdmin",
      requireAuth: true,
      element: <TestAdmin />,
   },
   {
      path: "/questionAdmin",
      requireAuth: true,
      element: <QuestionAdministration />,
   },
   {
      path: "/Test/:id",
      requireAuth: true,
      element: <UserTest />,
   },
   {
      path: "/Portal",
      requireAuth: true,
      element: <UserPortal />,
   },
   ...ApiAuthorzationRoutes,
];

export default AppRoutes;
