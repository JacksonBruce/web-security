import sys
import unittest
import tkinter
import ttk
from test.support import requires, run_unittest

import support

requires('gui')

class LabeledScaleTest(unittest.TestCase):

    def test_widget_destroy(self):
        # automatically created variable
        x = ttk.LabeledScale()
        var = x._variable._name
        x.destroy()
        self.failUnlessRaises(tkinter.TclError, x.tk.globalgetvar, var)

        # manually created variable
        myvar = tkinter.DoubleVar()
        name = myvar._name
        x = ttk.LabeledScale(variable=myvar)
        x.destroy()
        self.failUnlessEqual(x.tk.globalgetvar(name), myvar.get())
        del myvar
        self.failUnlessRaises(tkinter.TclError, x.tk.globalgetvar, name)

        # checking that the tracing callback is properly removed
        myvar = tkinter.IntVar()
        # LabeledScale will start tracing myvar
        x = ttk.LabeledScale(variable=myvar)
        x.destroy()
        # Unless the tracing callback was removed, creating a new
        # LabeledScale with the same var will cause an error now. This
        # happens because the variable will be set to (possibly) a new
        # value which causes the tracing callback to be called and then
        # it tries calling instance attributes not yet defined.
        ttk.LabeledScale(variable=myvar)
        if hasattr(sys, 'last_type'):
            self.failIf(sys.last_type == tkinter.TclError)


    def test_initialization(self):
        # master passing
        x = ttk.LabeledScale()
        self.failUnlessEqual(x.master, tkinter._default_root)
        x.destroy()
        master = tkinter.Frame()
        x = ttk.LabeledScale(master)
        self.failUnlessEqual(x.master, master)
        x.destroy()

        # variable initialization/passing
        passed_expected = ((2.5, 2), ('0', 0), (0, 0), (10, 10),
            (-1, -1), (sys.maxsize + 1, sys.maxsize + 1))
        for pair in passed_expected:
            x = ttk.LabeledScale(from_=pair[0])
            self.failUnlessEqual(x.value, pair[1])
            x.destroy()
        x = ttk.LabeledScale(from_='2.5')
        self.failUnlessRaises(ValueError, x._variable.get)
        x.destroy()
        x = ttk.LabeledScale(from_=None)
        self.failUnlessRaises(ValueError, x._variable.get)
        x.destroy()
        # variable should have its default value set to the from_ value
        myvar = tkinter.DoubleVar(value=20)
        x = ttk.LabeledScale(variable=myvar)
        self.failUnlessEqual(x.value, 0)
        x.destroy()
        # check that it is really using a DoubleVar
        x = ttk.LabeledScale(variable=myvar, from_=0.5)
        self.failUnlessEqual(x.value, 0.5)
        self.failUnlessEqual(x._variable._name, myvar._name)
        x.destroy()

        # widget positionment
        def check_positions(scale, scale_pos, label, label_pos):
            self.failUnlessEqual(scale.pack_info()['side'], scale_pos)
            self.failUnlessEqual(label.place_info()['anchor'], label_pos)
        x = ttk.LabeledScale(compound='top')
        check_positions(x.scale, 'bottom', x.label, 'n')
        x.destroy()
        x = ttk.LabeledScale(compound='bottom')
        check_positions(x.scale, 'top', x.label, 's')
        x.destroy()
        x = ttk.LabeledScale(compound='unknown') # invert default positions
        check_positions(x.scale, 'top', x.label, 's')
        x.destroy()
        x = ttk.LabeledScale() # take default positions
        check_positions(x.scale, 'bottom', x.label, 'n')
        x.destroy()

        # extra, and invalid, kwargs
        self.failUnlessRaises(tkinter.TclError, ttk.LabeledScale, a='b')


    def test_horizontal_range(self):
        lscale = ttk.LabeledScale(from_=0, to=10)
        lscale.pack()
        lscale.wait_visibility()
        lscale.update()

        linfo_1 = lscale.label.place_info()
        prev_xcoord = lscale.scale.coords()[0]
        self.failUnlessEqual(prev_xcoord, int(linfo_1['x']))
        # change range to: from -5 to 5. This should change the x coord of
        # the scale widget, since 0 is at the middle of the new
        # range.
        lscale.scale.configure(from_=-5, to=5)
        # The following update is needed since the test doesn't use mainloop,
        # at the same time this shouldn't affect test outcome
        lscale.update()
        curr_xcoord = lscale.scale.coords()[0]
        self.failUnless(prev_xcoord != curr_xcoord)
        # the label widget should have been repositioned too
        linfo_2 = lscale.label.place_info()
        self.failUnlessEqual(lscale.label['text'], 0)
        self.failUnlessEqual(curr_xcoord, int(linfo_2['x']))
        # change the range back
        lscale.scale.configure(from_=0, to=10)
        self.failUnless(prev_xcoord != curr_xcoord)
        self.failUnlessEqual(prev_xcoord, int(linfo_1['x']))

        lscale.destroy()


    def test_variable_change(self):
        x = ttk.LabeledScale()
        x.pack()
        x.wait_visibility()
        x.update()

        curr_xcoord = x.scale.coords()[0]
        newval = x.value + 1
        x.value = newval
        # The following update is needed since the test doesn't use mainloop,
        # at the same time this shouldn't affect test outcome
        x.update()
        self.failUnlessEqual(x.label['text'], newval)
        self.failUnless(x.scale.coords()[0] > curr_xcoord)
        self.failUnlessEqual(x.scale.coords()[0],
            int(x.label.place_info()['x']))

        # value outside range
        x.value = x.scale['to'] + 1 # no changes shouldn't happen
        x.update()
        self.failUnlessEqual(x.label['text'], newval)
        self.failUnlessEqual(x.scale.coords()[0],
            int(x.label.place_info()['x']))

        x.destroy()


    def test_resize(self):
        x = ttk.LabeledScale()
        x.pack(expand=True, fill='both')
        x.wait_visibility()
        x.update()

        width, height = x.master.winfo_width(), x.master.winfo_height()
        width, height = width * 2, height * 2

        x.value = 3
        x.update()
        x.master.wm_geometry("%dx%d" % (width, height))
        self.failUnlessEqual(int(x.label.place_info()['x']),
            x.scale.coords()[0])

        x.master.wm_geometry("%dx%d" % (width, height))
        x.destroy()


class OptionMenuTest(unittest.TestCase):

    def setUp(self):
        self.root = support.get_tk_root()
        self.textvar = tkinter.StringVar(self.root)

    def tearDown(self):
        del self.textvar
        self.root.destroy()


    def test_widget_destroy(self):
        var = tkinter.StringVar()
        optmenu = ttk.OptionMenu(None, var)
        name = var._name
        optmenu.update_idletasks()
        optmenu.destroy()
        self.failUnlessEqual(optmenu.tk.globalgetvar(name), var.get())
        del var
        self.failUnlessRaises(tkinter.TclError, optmenu.tk.globalgetvar, name)


    def test_initialization(self):
        self.failUnlessRaises(tkinter.TclError,
            ttk.OptionMenu, None, self.textvar, invalid='thing')

        optmenu = ttk.OptionMenu(None, self.textvar, 'b', 'a', 'b')
        self.failUnlessEqual(optmenu._variable.get(), 'b')

        self.failUnless(optmenu['menu'])
        self.failUnless(optmenu['textvariable'])

        optmenu.destroy()


    def test_menu(self):
        items = ('a', 'b', 'c')
        default = 'a'
        optmenu = ttk.OptionMenu(None, self.textvar, default, *items)
        found_default = False
        for i in range(len(items)):
            value = optmenu['menu'].entrycget(i, 'value')
            self.failUnlessEqual(value, items[i])
            if value == default:
                found_default = True
        self.failUnless(found_default)
        optmenu.destroy()

        # default shouldn't be in menu if it is not part of values
        default = 'd'
        optmenu = ttk.OptionMenu(None, self.textvar, default, *items)
        curr = None
        i = 0
        while True:
            last, curr = curr, optmenu['menu'].entryconfigure(i, 'value')
            if last == curr:
                # no more menu entries
                break
            self.failIf(curr == default)
            i += 1
        self.failUnlessEqual(i, len(items))

        # check that variable is updated correctly
        optmenu.pack()
        optmenu.wait_visibility()
        optmenu['menu'].invoke(0)
        self.failUnlessEqual(optmenu._variable.get(), items[0])

        # changing to an invalid index shouldn't change the variable
        self.failUnlessRaises(tkinter.TclError, optmenu['menu'].invoke, -1)
        self.failUnlessEqual(optmenu._variable.get(), items[0])

        optmenu.destroy()

        # specifying a callback
        success = []
        def cb_test(item):
            self.failUnlessEqual(item, items[1])
            success.append(True)
        optmenu = ttk.OptionMenu(None, self.textvar, 'a', command=cb_test,
            *items)
        optmenu['menu'].invoke(1)
        if not success:
            self.fail("Menu callback not invoked")

        optmenu.destroy()


tests = [LabeledScaleTest, OptionMenuTest]

if __name__ == "__main__":
    run_unittest(*tests)
