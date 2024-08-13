import {useEffect, useState} from "react";
import {AccountInfoResponse, auth} from "../../../api/auth.ts";
import {ChangePasswordForm} from "../components/ChangePasswordForm.tsx";
import {useSearchParams} from "react-router-dom";
import {useAuth} from "../../../providers/AuthProvider.tsx";
import {ExternalProviders} from "../../auth/components/ExternalProviders.tsx";

export const MyAccount = () => {
    const [user, setUser] = useState<AccountInfoResponse>();
    useEffect(() => {
        auth.accountInfo().then((res) => {
            setUser(res);
        });
    }, []);

    const [params] = useSearchParams();
    const link = params.get("link");
    const failed = params.get("failed");
    const authProvider = useAuth();
    useEffect(() => {
        if (!link) return;

        if (failed) {
            //TODO
        } else {
            authProvider.linkLogin().then(() => {
                debugger;
                auth.accountInfo().then((res) => {
                    setUser(res);
                });
            }).catch(() => {
                alert("Failed to link account");
            });
        }
    }, [link, failed]);

    return (<>
            <div>
                <div className="px-4 sm:px-0">
                    <h3 className="text-base font-semibold leading-7 text-gray-900">Profile Information</h3>
                </div>
                <div className="mt-6 border-t border-gray-100">
                    <dl className="divide-y divide-gray-100">
                        <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt className="text-sm font-medium leading-6 text-gray-900">Email address</dt>
                            <dd className="mt-1 text-sm leading-6 text-gray-700 sm:col-span-2 sm:mt-0">{user?.email}</dd>
                        </div>

                        <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt className="text-sm font-medium leading-6 text-gray-900">Logins</dt>
                            <dd className="mt-1 text-sm leading-6 text-gray-700 sm:col-span-2 sm:mt-0">{user?.logins.join(", ")}</dd>
                        </div>

                        <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt className="text-sm font-medium leading-6 text-gray-900">Link new logins</dt>
                            <dd className="mt-1 text-sm leading-6 text-gray-700 sm:col-span-2 sm:max-w-[480px]">
                                <ExternalProviders mode="LinkLogin"/>
                            </dd>
                        </div>

                        {user?.hasPassword && <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt className="text-sm font-medium leading-6 text-gray-900">Change password</dt>
                            <dd className="mt-1 text-sm leading-6 text-gray-700 sm:col-span-2 sm:mt-0">
                                <div className="mt-10 sm:w-full sm:max-w-[480px]">
                                    <ChangePasswordForm/>
                                </div>
                            </dd>
                        </div>}

                        <div className="px-4 py-6 sm:grid sm:grid-cols-3 sm:gap-4 sm:px-0">
                            <dt className="text-sm font-medium leading-6 text-gray-900">Roles</dt>
                            <dd className="mt-1 text-sm leading-6 text-gray-700 sm:col-span-2 sm:mt-0">{user?.roles.join(", ")}</dd>
                        </div>
                    </dl>
                </div>
            </div>
        </>
    )
};
