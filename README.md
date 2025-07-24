// TODO Elevate drive with credentials: from Commander branch CSharp

<!-- public static Result<Nothing, RequestError> ElevateDrive(Credentials credentials)
        => WNetAddConnection2(new()
            {
                Scope = ResourceScope.GlobalNetwork,
                ResourceType = ResourceType.Disk,
                DisplayType = ResourceDisplaytype.Share,
                RemoteName = credentials.Path
            }, credentials.Password, credentials.Name, 0) switch
        {
            0  => Ok<Nothing, RequestError>(nothing),
            67 => Error<Nothing, RequestError>(IOErrorType.NetNameNotFound.ToError()),
            5 or 86  => Error<Nothing, RequestError>(IOErrorType.WrongCredentials.ToError())
                            .SideEffect(_ => Events.Credentials(credentials.Path)),
            _  => Error<Nothing, RequestError>(IOErrorType.Exn.ToError())
        }; -->

// TODO create folder
// TODO rename file
// TODO rename with copy

// TODO Menu open explorer here

// TODO CopyConflicts: compare versions

// TODO Path in status bar
// TODO Menu commands like refresh executes a double click in folder view