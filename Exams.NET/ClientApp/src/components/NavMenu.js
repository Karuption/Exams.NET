import React, { Component } from "react";
import {
   Collapse,
   DropdownItem,
   DropdownMenu,
   DropdownToggle,
   Navbar,
   NavbarBrand,
   NavbarToggler,
   NavItem,
   NavLink,
   UncontrolledDropdown,
} from "reactstrap";
import { Link } from "react-router-dom";
import { LoginMenu } from "./api-authorization/LoginMenu";
import "./NavMenu.css";

export class NavMenu extends Component {
   static displayName = NavMenu.name;

   constructor(props) {
      super(props);

      this.toggleNavbar = this.toggleNavbar.bind(this);
      this.state = {
         collapsed: true,
      };
   }

   toggleNavbar() {
      this.setState({
         collapsed: !this.state.collapsed,
      });
   }

   render() {
      return (
         <header>
            <Navbar
               className='navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3'
               container
               light>
               <NavbarBrand tag={Link} to='/'>
                  Exams.NET
               </NavbarBrand>
               <NavbarToggler onClick={this.toggleNavbar} className='mr-2' />
               <Collapse
                  className='d-sm-inline-flex flex-sm-row-reverse'
                  isOpen={!this.state.collapsed}
                  navbar>
                  <ul className='navbar-nav flex-grow'>
                     <NavItem>
                        <NavLink tag={Link} className='text-dark' to='/'>
                           Home
                        </NavLink>
                     </NavItem>
                     <NavItem>
                        <NavLink tag={Link} className='text-dark' to='/Portal'>
                           Portal
                        </NavLink>
                     </NavItem>
                     <NavItem>
                        <UncontrolledDropdown nav inNavbar>
                           <DropdownToggle nav caret>
                              Administration
                           </DropdownToggle>
                           <DropdownMenu right>
                              <DropdownItem
                                 tag={Link}
                                 className='text-dark'
                                 to='/testAdmin'>
                                 Test Administration
                              </DropdownItem>
                              <DropdownItem
                                 tag={Link}
                                 className='text-dark'
                                 to='/questionAdmin'>
                                 Question Administration
                              </DropdownItem>
                           </DropdownMenu>
                        </UncontrolledDropdown>{" "}
                     </NavItem>
                     <LoginMenu></LoginMenu>
                  </ul>
               </Collapse>
            </Navbar>
         </header>
      );
   }
}
