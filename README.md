# City Sample on Unreal Engine 5

**City Sample** is a sample project on **WVS**, created by Epic Games. The project reveals how the city scene from The Matrix Awakens was built and consists of a city with crowds, buildings, and vehicles. The project is available for Unreal Engine 5.3, it comes equipped with an automated build flow, and is provided here as an example for the **WVS Community** to learn from.

> Additional information on **City Sample** can be found on [Epic Games](https://www.unrealengine.com/marketplace/en-US/product/city-sample)

WARNING:
**Please be aware of the file size of this project. Not Recommended for Free Tier users.**
<br>
![filesize](img/city-file-size.png)

### Benefits of Downloading the project on WVS

By starting with this project, you get the **City Sample** correctly configured in a repository, with automated Windows builds already
set up for you. An example project already in WVS will help eliminate the time you spend downloading from Epic Games and uploading to WVS. All you need to do to get a version in your repository is **Fork** the project, and you are ready to start working with it. We'll guide you through it below.


> Learn more about automated build [flows](https://docs.wvs.io/wiki/flows/overview.html), what build flows are available, and how to add new build flows to your project. 

### What's included

We've included an automated build flow in this project so you could try out. 

| Flow    | Description|
|---      | ---        |
| Windows Client  | Development Game Build for Windows |

## Summary of System Requirements
- Unreal Engine 5.3
- Windows - Any machine that is capable of running Unreal Engine can run this project.

## Fork the Project
When you **Fork** a project, it allows you to make a copy of the project and save it to your personal namespace or group. With your own copy, you can check in any changes back to the repository.
To get your own copy of the project in your own personal repo or group, fork the project by clicking on the **Fork** button near the middle of the main project page.

![fork](img/city-fork.png)

After you click the **Fork** button, you will be on the **Fork project** page. This page requires a couple of selections and inputs. Below, we describe each entry. 

![fork2](img/city-fork2.png)


- **Project name**  
The project name option auto-populates to the original name of the project, but you have the option to name it whatever you would like.

 > example: My Cool City Sample

- **Project URL**   
In the project URL section, you can select where you want your project to live. There are two options, your project can live in your ***personal*** **namespace**, and if you have multiple projects or are working with others, you can create a ***group*** **namespace**. In WVS, a namespace is a unique name for a user, a group, or a subgroup under which a project can be created.

 NOTE:
 Learn more in the [WVS DOCS section on Namespaces](https://docs.wvs.io/wiki/gl/user/group/index.html#namespaces).

- **Project slug**   
The project slug field is optional and can be left as the default, autogenerated slug. The project slug is usually the same as the project name but can be changed.

- **Product description**   
The project description field is another optional field. If you have multiple projects, it can be used to briefly describe your project.

- **Visibility level** 
The visibility level section sets the privacy level for your project. 

| Visibility level | Description |
|---|---|
|Private | Project access must be granted explicitly to each user. If this project is part of a group, access will be granted to members of the group.|
|Internal | The project can be accessed by any user logged into WVS. |
|Public   | The project can be accessed from the internet without any authentication.  |


Click **Fork Project** to complete the operation. **You now have your own copy of the project.**

> To learn more about Forking, go to our WVS Docs site, [Fork section](https://docs.wvs.io/wiki/Projects/Fork-Project.html) or get a [ Quick Tutorial](https://docs.wvs.io/wiki/quickstarts/fork-clone.html).


## Choose a Version Control Client

We offer an easy-to-use Git-based client for artists and less technical users. Try it out!

1. Download the WVS Desktop Client for Windows: [https://docs.wvs.io/wiki/downloads](https://docs.wvs.io/wiki/downloads)
2. Run the downloaded installer
3. Launch the **WVS Desktop Client**
4. Click the login button to log into the client. If you follow the prompt, it will open an authentication screen in your browser.
5. Log into WVS if you aren’t already.
6. Once complete, you may close that tab and return the WVS Desktop Client to see your avatar at the bottom left of the application.

> Learn to use the WVS Client by checking out this [Tutorial](https://docs.wvs.io/wiki/wvs-desktop-client.html).

WVS uses Git, and if you're familiar with Git, we suggest you use your favorite Git client, such as GitHub or Sourcetree, which works great with WVS.  

## Run Your Flows in WVS

Flows are units of automation that help you perform common tasks in your WVS project. Flows are the heart of WVS. We take project flows and form an automation pipeline without needing engineering support. This tutorial will demonstrate how easy it can be to get started with flows.

>Learn more about flows to get the most out of WVS in [WVS Docs](https://docs.wvs.io/wiki/flows/Flows.html).


### Windows 
Windows client and server builds have been pre-configured for you using our standard our Windows flows and do not require any special variables to configure. You can download the builds and directly install them on your machine. 

**Locate your builds**

To find your builds, go to **My Projects > View All Projects** and click on your project. From the left-hand nav, click on **Outputs**
In the **Builds** page, hover over **Complete** for the Windows build and locate the **Download** button on the far right, under **Actions** and download your build.

![download](img/downloads.png)

> If you click on the “Complete” button, it will take you to the flows page, where you can click on the appropriate flow and have the option to download or browse on the right-hand side. 

**Configure your machine**  
Once your installer is downloaded and extracted from the zip folder, you can click on the executable file to open your project in Windows. If you have Microsoft defender or a Third-party virus program, you may get a message informing you it is an unrecognized app and it was published by an unknown publisher. Since your project is not signed, it is a common message. You can select **Run Anyway**, and your project should appear.

### Kick off a Build

The Windows and Linux flows are setup on the project and ready for you to use. The flows are enabled by default.

1. In the WVS Web client, go to **My Projects > View All Projects** and click on your project.  
In the left-hand nav on your project page, click **Version control > Files.**

2. From the file list, click `README.md`.

3. Click the dropdown next to **Open in Web IDE.** 

![webide](img/webide.png)

4. Add a period at the end of `City Sample on Unreal Engine 5` or make any other change to the file and commit your change to the main branch.

 Your build should now trigger.

> Occassionally, you will see a successful job titled noop, this job is a system check and not your build.
> Successful builds will display your flow names.

## Troubleshooting
 > For Additional [Support](https://docs.wvs.io/wiki/Support), please reach out to us on our [Discord](https://discord.gg/c6JFTwbbvV) or by Email.


 ---

 [Report a problem with this project.](https://wvs.io/wvs-public/wvs-issues/-/issues?sort=created_date&state=opened)


