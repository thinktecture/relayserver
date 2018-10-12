(function () {
    "use strict";

    app.module.constant('translationsEn', {
        "COMMON": {
            "CREATE": "Create",
            "UPDATE": "Update",
            "CLOSE": "Close",
            "CANCEL": "Cancel",
            "USERNAME": "Username",
            "PASSWORD": "Password",
            "PASSWORD_OLD": "Current password",
            "DELETE": "Delete",
            "EDIT": "Edit",
            "DATE": "Date",
            "VERIFY_PASSWORD": "Verify Password",
            "REFRESH": "Refresh",
            "ERRORS": {
                "FIELD_REQUIRED": "This field is required.",
                "PASSWORDS_DO_NOT_MATCH": "Passwords do not match.",
                "USER_NAME_UNAVAILABLE": "This name is already taken."
            }
        },
        "MAIN": {
            "TITLE": "Thinktecture RelayServer Admin",
            "LOGOUT": "Logout",
            "DASHBOARD": "Dashboard",
            "LINKS": "Links",
            "USERS": "Users"
        },
        "DASHBOARD": {
            "TITLE": "Dashboard"
        },
        "LOGIN": {
            "TITLE": "Login",
            "LOGIN": "Login",
            "FAILED": "Credentials not found. Login not possible!",
            "REMEMBER_ME": "Remember me"
        },
        "SETUP": {
            "TITLE": "RelayServer first time setup",
            "DESCRIPTION": "Welcome to RelayServer first time setup. It seems that this is the first access to the admin interface. Please create your first admin user before logging in.",
            "CREATE_USER": "Create user",
            "USER_NOT_CREATED": "The user can't be created!",
            "USER_CREATED": "User was created successfully. You can now log in."
        },
        "LINKS": {
            "TITLE": "Links",
            "USERNAME": "Name",
            "SYMBOLIC_NAME": "Description",
            "STATE": "Link active",
            "CREATION_DATE": "Creation Date",
            "PING": "Ping",
            "CREATE_LINK": "Create Link",
            "MODAL_CREATE": {
                "TITLE": "Create New Link",
                "CREATED": "Your link has been created successfully. Please copy the password to authenticate your link with RelayServer:"
            },
            "PREVIOUS": "Previous",
            "NEXT": "Next",
            "FIRST": "First",
            "LAST": "Last",
            "SEARCH_TEXT": "Search text...",
            "IS_DISABLED": "No",
            "IS_ENABLED": "Yes",
            "NOTIFICATIONS": {
                "LINK_UPDATED_SUCCESS": "Link “{{linkName}}” has been updated successfully.",
                "LINK_UPDATED_ERROR": "Link “{{linkName}}” could not be updated.",
                "CREATE_SUCCESS": "Link created successfully.",
                "CREATE_ERROR": "Link could not be created."
            }
        },
        "LINK_DETAILS": {
            "TITLE": "Link: {{linkName}}",
            "NOT_FOUND": "Link was not found.",
            "DELETE": "Delete",
            "DELETE_SUCCESS": "Link successful deleted",
            "DELETE_UNSUCCESSFUL": "Link can't be deleted",
            "FORWARD_LOCAL_TARGET_ERROR_RESPONSE": "Forward internal server error contents",
            "FORWARD_LOCAL_TARGET_ERROR_RESPONSE_WARNING": "Be careful! This setting may leak stack traces, SQL statements or other sensitive information to the public.",
            "ALLOW_LOCAL_CLIENT_REQUESTS_ONLY": "Allow local client requests only",
            "MAXIMUM_LINKS": "Maximum links",
            "MAXIMUM_LINKS_UNLIMITED": "Unlimited",
            "IS_CONNECTED": "Connectivity",
            "CONNECTION_IDS": "Connection ID/IDs",
            "NO_LOGS_AVAILABLE": "No log entries available.",
            "NOTIFICATIONS": {
                "PING_LINK": "Pinging “{{symbolicName}}”…",
                "PING_SUCCESS": "“{{symbolicName}}” seems to be available.",
                "PING_ERROR": "“{{symbolicName}}” seems to be unavailable.",
                "LOG_ERROR": "Log could not be retrieved."
            },
            "SERIES": {
                "CONTENT_IN": "Content in",
                "CONTENT_OUT": "Content out"
            },
            "RESOLUTIONS": {
                "DAILY": "Daily",
                "MONTHLY": "Monthly",
                "YEARLY": "Yearly"
            },
            "TABS": {
                "LOGS": "Logs",
                "CHARTS": "Charts",
                "INFO": "Info",
                "TRACE": "Trace"
            },
            "TRACE": {
                "START": "Start tracing ({{minutes}} minutes)",
                "STOP": "Stop tracing ({{time}} left)",
                "MINUTES": "Minutes",
                "MINUTES_LABEL": "Runtime (in minutes)",
                "DESCRIPTION": "Use the following form to start tracing a link. RelayServer will trace every HTTP request and response for the given link. " +
                               "It will save both header and content to disk for further investigation. ",
                "LOGS": "Logs",
                "SHOW_RESULTS": "Show results",
                "TRACE_RESULT": "Trace result for '{{id}}'",
                "REQUEST_HEADER": "Request header",
                "RESPONSE_HEADER": "Response header",
                "REQUEST_CONTENT": "Request content",
                "RESPONSE_CONTENT": "Response content"
            },
            "MODAL_VIEW": {
                "TITLE": "Content"
            },
            "MODAL_DELETE": {
                "TITLE": "Confirm delete",
                "TEXT": "Do you really want to delete “{{linkName}}”?"
            },
            "NO_DATA": "No data for selected range available."
        },
        "USERS": {
            "TITLE": "Users",
            "ADD_USER": "Add User",
            "OPTIONS": "Options",
            "LOCKEDOUT_UNTIL": "Locked until",
            "NOTIFICATIONS": {
                "CREATE_SUCCESS": "User created successfully.",
                "CREATE_ERROR": "User could not be created.",
                "DELETE_SUCCESS": "User deleted successfully.",
                "DELETE_ERROR": "User could not be deleted.",
                "EDIT_PASSWORD_SUCCESS": "Password changed successfully.",
                "EDIT_PASSWORD_ERROR": "Password could not be changed."
            },
            "MODAL_CREATE": {
                "TITLE": "Add New User"
            },
            "MODAL_DELETE": {
                "TITLE": "Delete User",
                "DESCRIPTION": "Do you really want to delete the user “{{userName}}”?"
            },
            "MODAL_EDIT_PASSWORD": {
                "TITLE": "Edit Password",
                "DESCRIPTION": "Please enter a new password for user “{{userName}}”."
            }
        }
    });
})();
